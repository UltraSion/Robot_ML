using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotML.Candidates
{
    public sealed class CandidateGalleryControls : MonoBehaviour
    {
        public string galleryTitle;
        public string caveat;
        public CandidateRobotAgent[] candidates;
        public Camera viewCamera;
        public Vector3 overviewPosition;
        public Vector3 overviewRotation;
        int selected;
        bool paused;

        void Start() { if (candidates != null && candidates.Length > 0) Focus(0); }

        public void Focus(int index)
        {
            selected = (index + candidates.Length) % candidates.Length;
            var root = candidates[selected].rootBody;
            var renderers = root.GetComponentsInChildren<Renderer>();
            var bounds = new Bounds(root.transform.position, Vector3.one);
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            }
            var point = bounds.center;
            float distance = Mathf.Max(3, bounds.size.magnitude * 1.6f);
            viewCamera.orthographic = false;
            viewCamera.transform.position = point + new Vector3(-1, 0.7f, -1).normalized * distance;
            viewCamera.transform.LookAt(point);
        }

        public void Overview()
        {
            viewCamera.transform.position = overviewPosition;
            viewCamera.transform.rotation = Quaternion.Euler(overviewRotation);
        }

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null)
            {
                if (keyboard.fKey.wasPressedThisFrame) Focus(selected);
                if (keyboard.tabKey.wasPressedThisFrame) Focus(selected + 1);
                if (keyboard.rKey.wasPressedThisFrame) foreach (var robot in candidates) robot.EndEpisode();
            }
            if (mouse == null || !mouse.rightButton.isPressed || keyboard == null) return;
            var delta = mouse.delta.ReadValue() * 0.12f;
            var angles = viewCamera.transform.eulerAngles;
            viewCamera.transform.rotation = Quaternion.Euler(angles.x - delta.y, angles.y + delta.x, 0);
            var move = Vector3.zero;
            if (keyboard.wKey.isPressed) move += Vector3.forward;
            if (keyboard.sKey.isPressed) move += Vector3.back;
            if (keyboard.aKey.isPressed) move += Vector3.left;
            if (keyboard.dKey.isPressed) move += Vector3.right;
            if (keyboard.eKey.isPressed) move += Vector3.up;
            if (keyboard.qKey.isPressed) move += Vector3.down;
            viewCamera.transform.Translate(move * (keyboard.leftShiftKey.isPressed ? 25 : 10) * Time.unscaledDeltaTime);
#else
            if (Input.GetKeyDown(KeyCode.F)) Focus(selected);
            if (Input.GetKeyDown(KeyCode.Tab)) Focus(selected + 1);
            if (Input.GetKeyDown(KeyCode.R)) foreach (var robot in candidates) robot.EndEpisode();
            if (!Input.GetMouseButton(1)) return;
            var angles = viewCamera.transform.eulerAngles;
            viewCamera.transform.rotation = Quaternion.Euler(angles.x - Input.GetAxis("Mouse Y") * 3,
                angles.y + Input.GetAxis("Mouse X") * 3, 0);
            var move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) move += Vector3.back;
            if (Input.GetKey(KeyCode.A)) move += Vector3.left;
            if (Input.GetKey(KeyCode.D)) move += Vector3.right;
            if (Input.GetKey(KeyCode.E)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) move += Vector3.down;
            viewCamera.transform.Translate(move * (Input.GetKey(KeyCode.LeftShift) ? 25 : 10) * Time.unscaledDeltaTime);
#endif
        }

        void OnGUI()
        {
            if (candidates == null || candidates.Length == 0) return;
            GUILayout.BeginArea(new Rect(12, 12, 620, 240), GUI.skin.box);
            GUILayout.Label(galleryTitle + "  |  " + candidates.Length + " candidates");
            var robot = candidates[selected];
            GUILayout.Label((selected + 1) + "/" + candidates.Length + "  " + robot.modelPath);
            GUILayout.Label($"Observation {robot.lastObservationCount}/{robot.observationCount} | Action {robot.actionCount} | Decisions {robot.receivedActions} | Speed {robot.observedSpeed:F2}");
            GUILayout.Label("Targets reached: " + robot.targetsReached);
            GUILayout.Label(caveat);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous")) Focus(selected - 1);
            if (GUILayout.Button("Next / Tab")) Focus(selected + 1);
            if (GUILayout.Button("Focus / F")) Focus(selected);
            if (GUILayout.Button("Overview")) Overview();
            if (GUILayout.Button("Reset all / R")) foreach (var candidate in candidates) candidate.EndEpisode();
            if (GUILayout.Button(paused ? "Resume" : "Pause")) { paused = !paused; Time.timeScale = paused ? 0 : 1; }
            GUILayout.EndHorizontal();
            GUILayout.Label("Right mouse + WASD: move camera | Q/E: down/up | Shift: faster");
            GUILayout.Label("Target speed: " + robot.targetSpeed.ToString("F1"));
            float speed = GUILayout.HorizontalSlider(robot.targetSpeed, 0.2f, 8);
            foreach (var candidate in candidates) candidate.targetSpeed = speed;
            GUILayout.EndArea();
        }

        void OnDisable() { Time.timeScale = 1; }
    }
}
