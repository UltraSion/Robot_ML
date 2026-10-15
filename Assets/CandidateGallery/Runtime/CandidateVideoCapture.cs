using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace RobotML.Candidates
{
    [Serializable]
    public class CandidateVideoResult
    {
        public string modelPath, videoPath, error;
        public int sceneNumber, modelNumber, frames, segments, retries, episodeCount, maxStep;
        public float durationSeconds, simulatedSeconds, targetSpeed, movingFrameRatio, meanHorizontalSpeed;
        public bool success;
    }

    public sealed class CandidateVideoCapture : MonoBehaviour
    {
        public CandidateRobotAgent robot;
        public Camera captureCamera;
        public string ffmpegPath, outputPath;
        public int sceneNumber, modelNumber;
        public bool finished;
        public CandidateVideoResult result;
        const int Width = 1280, Height = 720, Fps = 30, FrameLimit = 600;
        Process encoder;
        RenderTexture renderTarget;
        Texture2D pixels;
        byte[] pixelBuffer;
        Text title, statistics;
        StreamWriter telemetry;
        Vector3 cameraOffset, smoothPosition, previousPosition;
        float startTime, lastUsefulMotion, lastRetryTime, speedSum;
        int frames, movingFrames, segments, retries, activeEpisode, movingStreak, stillFrames;
        bool recordingSegment;
        string encoderError = "";

        IEnumerator Start()
        {
            Time.timeScale = 1;
            Time.captureFramerate = Fps;
            QualitySettings.vSyncCount = 0;
            robot.targetSpeed = 8;
            robot.MaxStep = 100000;
            startTime = lastUsefulMotion = lastRetryTime = Time.time;
            activeEpisode = robot.completedEpisodes;
            previousPosition = robot.rootBody.transform.position;
            var bounds = RobotBounds();
            cameraOffset = new Vector3(-1, 0.7f, -1).normalized * Mathf.Max(3.5f, bounds.size.magnitude * 1.9f);
            smoothPosition = bounds.center;
            renderTarget = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            renderTarget.Create();
            pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            pixelBuffer = new byte[Width * Height * 3];
            captureCamera.targetTexture = renderTarget;
            CreateOverlay();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            telemetry = new StreamWriter(Path.ChangeExtension(outputPath, ".csv"));
            telemetry.WriteLine("frame,simulation_seconds,episode,step,horizontal_speed,targets_reached,segment");
            UnityEngine.Debug.Log($"CAPTURE_START scene={sceneNumber} model={modelNumber} path={robot.modelPath}");
            while (!finished)
            {
                // Explicit Camera.Render works in batch mode; WaitForEndOfFrame
                // can stop advancing when the Editor has no visible Game view.
                yield return null;
                try { Tick(); }
                catch (Exception error) { Complete(false, error.ToString()); }
            }
        }

        Bounds RobotBounds()
        {
            var renderers = robot.rootBody.GetComponentsInChildren<Renderer>();
            var bounds = new Bounds(robot.rootBody.transform.position, Vector3.one);
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        void Tick()
        {
            robot.targetSpeed = 8;
            var position = robot.rootBody.transform.position;
            bool reset = activeEpisode != robot.completedEpisodes;
            if (reset)
            {
                activeEpisode = robot.completedEpisodes;
                previousPosition = position;
                recordingSegment = false; movingStreak = stillFrames = 0;
                lastRetryTime = Time.time;
                smoothPosition = RobotBounds().center;
            }
            var velocity = robot.rootBody.linearVelocity;
            float speed = new Vector2(velocity.x, velocity.z).magnitude;
            var traveled = position - previousPosition; traveled.y = 0;
            float measuredSpeed = traveled.magnitude * Fps;
            previousPosition = position;
            bool moving = !reset && speed > 0.18f && measuredSpeed > 0.12f &&
                Vector3.Dot(robot.rootBody.transform.up, Vector3.up) > 0.2f;
            movingStreak = moving ? movingStreak + 1 : 0;
            stillFrames = moving ? 0 : stillFrames + 1;
            if (moving) lastUsefulMotion = Time.time;
            if (!recordingSegment && movingStreak >= 15)
            {
                recordingSegment = true;
                segments++;
                smoothPosition = RobotBounds().center;
                UnityEngine.Debug.Log($"CAPTURE_MOTION scene={sceneNumber} segment={segments} frames={frames} speed={speed:F2}");
            }
            if (recordingSegment && stillFrames > 12) recordingSegment = false;
            if (Time.time - Mathf.Max(lastUsefulMotion, lastRetryTime) > 10)
            {
                retries++;
                lastRetryTime = Time.time;
                recordingSegment = false; movingStreak = stillFrames = 0;
                var point = UnityEngine.Random.insideUnitCircle.normalized * 6;
                var targetPosition = robot.target.localPosition;
                robot.target.localPosition = new Vector3(point.x, targetPosition.y, point.y);
                robot.EndEpisode();
                UnityEngine.Debug.Log($"CAPTURE_RETRY scene={sceneNumber} retries={retries} frames={frames}");
            }
            smoothPosition = Vector3.Lerp(smoothPosition, RobotBounds().center, 0.2f);
            captureCamera.transform.position = smoothPosition + cameraOffset;
            captureCamera.transform.LookAt(smoothPosition);
            statistics.text = $"Target 8.0 m/s     Speed {speed:F2} m/s     Step {robot.StepCount:N0} / 100,000     Targets {robot.targetsReached}";
            if (recordingSegment)
            {
                if (encoder == null) StartEncoder();
                Canvas.ForceUpdateCanvases();
                captureCamera.Render();
                var previous = RenderTexture.active;
                RenderTexture.active = renderTarget;
                pixels.ReadPixels(new Rect(0,0,Width,Height),0,0,false);
                pixels.Apply(false);
                RenderTexture.active = previous;
                pixels.GetRawTextureData<byte>().CopyTo(pixelBuffer);
                encoder.StandardInput.BaseStream.Write(pixelBuffer, 0, pixelBuffer.Length);
                telemetry.WriteLine(string.Join(",", frames, (Time.time-startTime).ToString("F3",CultureInfo.InvariantCulture),
                    robot.completedEpisodes, robot.StepCount, speed.ToString("F4",CultureInfo.InvariantCulture), robot.targetsReached, segments));
                frames++;
                if (moving) movingFrames++;
                speedSum += speed;
                if (frames >= FrameLimit) { Complete(true, ""); return; }
            }
            if (Time.time - startTime > 360 || retries > 30)
                Complete(false, "Movement budget exhausted before collecting 20 seconds.");
        }

        void StartEncoder()
        {
            var start = new ProcessStartInfo(ffmpegPath) {
                UseShellExecute=false, CreateNoWindow=true, RedirectStandardInput=true, RedirectStandardError=true,
                Arguments=$"-hide_banner -loglevel error -y -f rawvideo -pixel_format rgb24 -video_size {Width}x{Height} -framerate {Fps} -i pipe:0 -vf vflip -c:v libx264 -preset fast -crf 20 -pix_fmt yuv420p -movflags +faststart -an \"{outputPath}\""
            };
            encoder = Process.Start(start);
            encoder.ErrorDataReceived += (_, args) => { if (args.Data != null) encoderError += args.Data + "\n"; };
            encoder.BeginErrorReadLine();
        }

        void CreateOverlay()
        {
            var canvasObject = new GameObject("Recording overlay", typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = captureCamera; canvas.planeDistance = 0.5f;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(Width,Height);
            title = MakeText(canvas.transform, new Vector2(24,-20), 28);
            title.fontStyle = FontStyle.Bold;
            title.text = $"SCENE {sceneNumber:00}  /  MODEL {modelNumber:00}";
            statistics = MakeText(canvas.transform, new Vector2(24,-58), 20);
            var footer = MakeText(canvas.transform, new Vector2(24,-Height+42), 17);
            footer.text = "Motion-selected footage  |  Stationary starts excluded  |  30 fps";
        }

        static Text MakeText(Transform parent, Vector2 position, int size)
        {
            var go = new GameObject("Caption", typeof(RectTransform), typeof(Text), typeof(Shadow));
            go.transform.SetParent(parent,false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(Width-40,40);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.color = Color.white; text.alignment = TextAnchor.UpperLeft;
            return text;
        }

        void Complete(bool success, string error)
        {
            if (finished) return;
            if (encoder != null)
            {
                encoder.StandardInput.Close();
                if (!encoder.WaitForExit(15000)) { encoder.Kill(); success=false; error += " Encoder timeout."; }
                else if (encoder.ExitCode != 0) { success=false; error += encoderError; }
                encoder.Dispose(); encoder=null;
            }
            telemetry?.Dispose(); telemetry=null;
            result = new CandidateVideoResult { sceneNumber=sceneNumber, modelNumber=modelNumber,
                modelPath=robot.modelPath, videoPath=outputPath, frames=frames, durationSeconds=(float)frames/Fps,
                simulatedSeconds=Time.time-startTime, targetSpeed=8, maxStep=robot.MaxStep,
                segments=segments, retries=retries, episodeCount=robot.completedEpisodes,
                movingFrameRatio=frames==0 ? 0 : (float)movingFrames/frames,
                meanHorizontalSpeed=frames==0 ? 0 : speedSum/frames, success=success, error=error };
            File.WriteAllText(Path.ChangeExtension(outputPath,".json"), JsonUtility.ToJson(result,true));
            UnityEngine.Debug.Log("CAPTURE_COMPLETE " + JsonUtility.ToJson(result));
            Time.captureFramerate = 0;
            finished=true;
        }

        void OnDestroy()
        {
            telemetry?.Dispose();
            if (encoder != null) { try { encoder.StandardInput.Close(); encoder.WaitForExit(3000); } catch { } encoder.Dispose(); }
            if (captureCamera != null) captureCamera.targetTexture=null;
            if (renderTarget != null) { renderTarget.Release(); Destroy(renderTarget); }
            if (pixels != null) Destroy(pixels);
            Time.captureFramerate=0;
        }
    }
}
