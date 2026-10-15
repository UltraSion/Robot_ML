using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RobotML.Candidates;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CandidateSceneValidation
{
    const string Key = "RobotML.CandidateValidation.";
    [Serializable] class Results { public Outcome[] scenes; }
    [Serializable] class Outcome
    {
        public string scene;
        public int robots, minimumActionCallbacks, minimumObservationCount;
        public bool passed;
        public string[] errors;
    }
    static readonly string[] Scenes = {
        "01_ForcedQuad_113_18", "02_Bipedal_191_22", "03_LegacyBipedal_175_20",
        "04_LegacyBipedal_158_21", "05_LegacyQuad_126_32",
        "06_SQW04_Order1234_113_32", "07_SQW04_Order4123_113_32" };

    static CandidateSceneValidation()
    {
        EditorApplication.update += Update;
        Application.logMessageReceived += OnLog;
    }

    public static void Run()
    {
        SessionState.SetBool(Key+"running", true);
        SessionState.SetInt(Key+"index", 0);
        SessionState.SetInt(Key+"phase", 0);
        SessionState.SetString(Key+"results", "{\"scenes\":[]}");
        SessionState.SetString(Key+"errors", "");
        SessionState.SetBool(Key+"failed", false);
    }

    static void OnLog(string message, string stack, LogType type)
    {
        if (!SessionState.GetBool(Key+"running", false) ||
            SessionState.GetInt(Key+"phase", 0) != 1 ||
            (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)) return;
        var errors = SessionState.GetString(Key+"errors", "");
        if (errors.Length < 12000) SessionState.SetString(Key+"errors", errors + message + "\n");
    }

    static void Update()
    {
        try { Tick(); }
        catch (Exception error)
        {
            SessionState.SetBool(Key+"running", false);
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    static void Tick()
    {
        if (!SessionState.GetBool(Key+"running", false)) return;
        int index = SessionState.GetInt(Key+"index", 0);
        int phase = SessionState.GetInt(Key+"phase", 0);
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (index == Scenes.Length)
        {
            File.WriteAllText("docs/onnx-scene-validation.json", SessionState.GetString(Key+"results", ""));
            SessionState.SetBool(Key+"running", false);
            bool failed = SessionState.GetBool(Key+"failed", false);
            Debug.Log(failed ? "CANDIDATE_VALIDATION_FAILED" : "CANDIDATE_VALIDATION_PASSED: all 7 scenes executed model actions");
            EditorApplication.Exit(failed ? 1 : 0);
            return;
        }
        if (phase == 0 && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            var path = "Assets/Scenes/ONNXCandidates/" + Scenes[index] + ".unity";
            EditorSceneManager.OpenScene(path);
            ValidateStructure();
            RenderPreview(Scenes[index]);
            SessionState.SetString(Key+"errors", "");
            SessionState.SetFloat(Key+"start", (float)EditorApplication.timeSinceStartup);
            SessionState.SetInt(Key+"phase", 1);
            EditorApplication.EnterPlaymode();
        }
        else if (phase == 1 && EditorApplication.isPlaying)
        {
            var robots = UnityEngine.Object.FindObjectsByType<CandidateRobotAgent>(FindObjectsSortMode.None);
            bool ready = robots.Length > 0 && robots.All(r=>r.receivedActions >= 100);
            bool timeout = EditorApplication.timeSinceStartup - SessionState.GetFloat(Key+"start", 0) > 45;
            string errors = SessionState.GetString(Key+"errors", "");
            if (!ready && !timeout && errors.Length == 0) return;
            var issues = errors.Split('\n').Where(s=>s.Length>0).ToList();
            foreach (var robot in robots)
            {
                if (robot.receivedActions < 100) issues.Add(robot.modelPath + ": insufficient action callbacks " + robot.receivedActions);
                if (robot.lastObservationCount != robot.observationCount) issues.Add(robot.modelPath + ": observation mismatch");
                if (!robot.outputsFinite) issues.Add(robot.modelPath + ": non-finite action");
                if (!float.IsFinite(robot.rootBody.transform.position.x) || !float.IsFinite(robot.rootBody.transform.position.y))
                    issues.Add(robot.modelPath + ": invalid physical state");
            }
            var outcome = new Outcome { scene=Scenes[index], robots=robots.Length,
                minimumActionCallbacks=robots.Length==0 ? 0 : robots.Min(r=>r.receivedActions),
                minimumObservationCount=robots.Length==0 ? 0 : robots.Min(r=>r.lastObservationCount),
                passed=issues.Count==0 && ready, errors=issues.ToArray() };
            var previous = JsonUtility.FromJson<Results>(SessionState.GetString(Key+"results", ""));
            var outcomes = previous.scenes.ToList(); outcomes.Add(outcome);
            SessionState.SetString(Key+"results", JsonUtility.ToJson(new Results { scenes=outcomes.ToArray() }, true));
            if (!outcome.passed) SessionState.SetBool(Key+"failed", true);
            Debug.Log("CANDIDATE_SCENE_CHECK " + Scenes[index] + " passed=" + outcome.passed + " robots=" + robots.Length);
            SessionState.SetInt(Key+"phase", 2);
            EditorApplication.ExitPlaymode();
        }
        else if (phase == 2 && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SessionState.SetInt(Key+"index", index+1);
            SessionState.SetInt(Key+"phase", 0);
        }
    }

    static void ValidateStructure()
    {
        var robots = UnityEngine.Object.FindObjectsByType<CandidateRobotAgent>(FindObjectsSortMode.None);
        foreach (var robot in robots)
        {
            var policy = robot.GetComponent<BehaviorParameters>();
            if (policy.Model == null || policy.BehaviorType != BehaviorType.InferenceOnly ||
                policy.BrainParameters.VectorObservationSize != robot.observationCount ||
                policy.BrainParameters.ActionSpec.NumContinuousActions != robot.actionCount)
                throw new InvalidOperationException("Invalid candidate model wiring: " + robot.name);
            if (robot.joints.Any(j=>j==null)) throw new InvalidOperationException("Missing articulation: " + robot.name);
        }
        if (robots.Length == 0 || UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Length < robots.Length)
            throw new InvalidOperationException("Missing robots or floor colliders");
    }

    static void RenderPreview(string name)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        Directory.CreateDirectory("docs/scene-previews");
        var camera = Camera.main;
        var buffer = new RenderTexture(1600, 900, 24);
        camera.targetTexture = buffer;
        var old = RenderTexture.active;
        try
        {
            camera.Render(); RenderTexture.active = buffer;
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0,0,1600,900),0,0); image.Apply();
            File.WriteAllBytes("docs/scene-previews/"+name+".png", image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
        }
        finally { camera.targetTexture=null; RenderTexture.active=old; buffer.Release(); UnityEngine.Object.DestroyImmediate(buffer); }
    }

    public static void FocusPreviewBatch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/ONNXCandidates/01_ForcedQuad_113_18.unity");
        var controls = UnityEngine.Object.FindFirstObjectByType<CandidateGalleryControls>();
        controls.Focus(0);
        RenderPreview("01_ForcedQuad_113_18_Focus");
        controls.Overview();
        Debug.Log("CANDIDATE_FOCUS_PREVIEW_SAVED");
    }
}
