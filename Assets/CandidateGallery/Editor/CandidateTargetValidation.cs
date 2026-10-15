using System;
using System.IO;
using RobotML.Candidates;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CandidateTargetValidation
{
    [MenuItem("Robot ML/Validate random target relocation")]
    public static void Validate()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run target validation in Edit Mode.");
        var scene = EditorSceneManager.NewPreviewScene();
        var savedRandom = UnityEngine.Random.state;
        int relocations = 0;
        try
        {
            UnityEngine.Random.InitState(1931);
            var tile = new GameObject("Validation tile");
            SceneManager.MoveGameObjectToScene(tile, scene);
            tile.transform.position = new Vector3(72, 0, 90);
            var robot = new GameObject("Validation robot");
            SceneManager.MoveGameObjectToScene(robot, scene);
            robot.transform.SetParent(tile.transform, false);
            robot.transform.localPosition = new Vector3(0, 3, 0);
            var body = robot.AddComponent<ArticulationBody>();
            var agent = robot.AddComponent<CandidateRobotAgent>();
            var goal = new GameObject("Validation target");
            SceneManager.MoveGameObjectToScene(goal, scene);
            goal.transform.SetParent(tile.transform, false);
            agent.rootBody = body; agent.target = goal.transform;
            goal.transform.localPosition = new Vector3(0, 0.06f, 6);
            var farTarget = goal.transform.position;
            Require(!agent.TryAdvanceTarget(), "A distant target must not move.");
            Require(goal.transform.position == farTarget && agent.targetsReached == 0, "Distant target changed.");
            for (int i = 0; i < 1000; i++)
            {
                var reached = goal.transform.position;
                body.transform.position = new Vector3(reached.x, 3, reached.z);
                Require(agent.TryAdvanceTarget(), "Arrival failed to relocate target.");
                var local = goal.transform.localPosition;
                Require(new Vector2(local.x, local.z).magnitude <= 6.001f, "Target left its tile bounds.");
                Require(Mathf.Abs(local.y - 0.06f) < 0.0001f, "Target height changed.");
                var separation = goal.transform.position - body.transform.position; separation.y = 0;
                Require(separation.magnitude >= 1.999f, "Next target too close to robot.");
                Require(!agent.TryAdvanceTarget(), "New target immediately counted as reached again.");
                relocations++;
            }
            Require(agent.targetsReached == 1000, "Arrival count mismatch.");
            Debug.Log("CANDIDATE_TARGET_VALIDATION_PASSED: " + relocations + " arrivals, separated targets, tile bounds and height preserved");
        }
        finally
        {
            UnityEngine.Random.state = savedRandom;
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    public static void ValidateBatch()
    {
        try { Validate(); EditorApplication.Exit(0); }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }
    static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
