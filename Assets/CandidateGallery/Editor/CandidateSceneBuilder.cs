using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controller;
using RobotML.Candidates;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CandidateSceneBuilder
{
    [Serializable] public class Manifest { public ModelEntry[] models; }
    [Serializable] public class ModelEntry
    {
        public string path, group;
        public int observations, continuous_actions;
    }
    [Serializable] public class SceneRecord
    {
        public string scene, group, caveat;
        public string[] models;
        public int robotCount;
    }
    [Serializable] public class BuildReport { public SceneRecord[] scenes; public int modelFiles, robotInstances; }
    class Gallery
    {
        public string name, group, prefab, commit, caveat;
        public CandidateContract contract;
        public bool oldLegOrder;
    }
    const string SceneFolder = "Assets/Scenes/ONNXCandidates";
    const string MaterialFolder = "Assets/CandidateGallery/Materials";
    const string LegacyBipedal = "Assets/Prefabs/GGangGGangIMaster.prefab";
    const string CurrentBipedal = "Assets/Prefabs/LBWSrEr_4LADAPTIVE.prefab";
    const string Quad = "Assets/AI/SQW04/SymetryQudPed.prefab";
    static Material floorMaterial, robotMaterial, targetMaterial;

    [MenuItem("Robot ML/Build ONNX candidate scenes")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(SceneFolder);
        Directory.CreateDirectory(MaterialFolder);
        AssetDatabase.Refresh();
        floorMaterial = MakeMaterial("Floor", new Color(0.18f, 0.22f, 0.29f));
        robotMaterial = MakeMaterial("Robot", new Color(0.7f, 0.8f, 0.9f));
        targetMaterial = MakeMaterial("Target", new Color(0.2f, 0.9f, 0.55f));
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("docs/onnx-candidates/manifest.json"));
        var galleries = new[]
        {
            new Gallery { name="01_ForcedQuad_113_18", group="forced_quad_113_18", contract=CandidateContract.Forced113,
                prefab="Assets/AI/SQW04_Forced/SymetryQudPed_Force.prefab", commit="2f873f7",
                caveat="Current contract. Inference only; flat floor, fixed target speed." },
            new Gallery { name="02_Bipedal_191_22", group="bipedal_191_22", contract=CandidateContract.Bipedal191,
                prefab=CurrentBipedal, commit="80333ed", caveat="Current 11-joint contract; original physical prefab preserved." },
            new Gallery { name="03_LegacyBipedal_175_20", group="bipedal_175_20", contract=CandidateContract.Bipedal175,
                prefab=LegacyBipedal, commit="5a2aec4", caveat="Historical 10-joint adapter. Matching prefab GUIDs; training commit unverified." },
            new Gallery { name="04_LegacyBipedal_158_21", group="bipedal_158_21", contract=CandidateContract.Bipedal158,
                prefab=LegacyBipedal, commit="7723886", caveat="Exploratory: 10-joint proxy robot. Historical forceUsage action restored; original physics unknown." },
            new Gallery { name="05_LegacyQuad_126_32", group="quad_126_32", contract=CandidateContract.Quad126,
                prefab=Quad, commit="c0f9018", caveat="Exploratory: four foot joints reconstructed. Historical physical settings unavailable." },
            new Gallery { name="06_SQW04_Order1234_113_32", group="quad_113_32", contract=CandidateContract.Quad113,
                prefab=Quad, commit="da9c6aa", oldLegOrder=true, caveat="Partial match: leg order 1,2,3,4. Only first 24 of 32 actions are consumed." },
            new Gallery { name="07_SQW04_Order4123_113_32", group="quad_113_32", contract=CandidateContract.Quad113,
                prefab=Quad, commit="f1ef807", caveat="Partial match: leg order 4,1,2,3. Only first 24 of 32 actions are consumed." }
        };
        var records = new List<SceneRecord>();
        foreach (var gallery in galleries)
        {
            var models = manifest.models.Where(m => m.group == gallery.group).ToArray();
            BuildScene(gallery, models);
            records.Add(new SceneRecord { scene=SceneFolder + "/" + gallery.name + ".unity", group=gallery.group,
                caveat=gallery.caveat, models=models.Select(m => m.path).ToArray(), robotCount=models.Length });
        }
        EditorBuildSettings.scenes = records.Select(r => new EditorBuildSettingsScene(r.scene, true)).ToArray();
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("docs");
        File.WriteAllText("docs/onnx-candidate-scenes.json", JsonUtility.ToJson(new BuildReport {
            scenes=records.ToArray(), modelFiles=manifest.models.Length, robotInstances=records.Sum(r=>r.robotCount) }, true));
        EditorSceneManager.OpenScene(records[0].scene);
        Debug.Log("CANDIDATE_SCENES_BUILT: 7 scenes, 62 model files, 63 robot instances");
    }

    public static void BuildBatch()
    {
        try { BuildAll(); EditorApplication.Exit(0); }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    public static void OpenDefaultScene()
    {
        EditorSceneManager.OpenScene(SceneFolder + "/01_ForcedQuad_113_18.unity");
    }

    static Material MakeMaterial(string name, Color color)
    {
        var path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, path); }
        material.color = color;
        return material;
    }

    static void BuildScene(Gallery gallery, ModelEntry[] models)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.65f, 0.7f, 0.8f);
        RenderSettings.skybox = null;
        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.2f; sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50, -35, 0);
        int columns = Mathf.Min(6, Mathf.CeilToInt(Mathf.Sqrt(models.Length)));
        int rows = Mathf.CeilToInt((float)models.Length / columns);
        var robots = new List<CandidateRobotAgent>();
        for (int i = 0; i < models.Length; i++)
        {
            var position = new Vector3((i % columns) * 18, 0, (i / columns) * 18);
            var tile = new GameObject($"Candidate_{i+1:00}");
            tile.transform.position = position;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor"; floor.transform.SetParent(tile.transform);
            floor.transform.localPosition = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(16, 0.5f, 16);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
            var goal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goal.name = "Target"; goal.transform.SetParent(tile.transform);
            goal.transform.localPosition = new Vector3(0, 0.06f, 6);
            goal.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            UnityEngine.Object.DestroyImmediate(goal.GetComponent<Collider>());
            goal.GetComponent<Renderer>().sharedMaterial = targetMaterial;
            var agent = BuildRobot(gallery, models[i], tile.transform, goal.transform);
            robots.Add(agent);
            var labelObject = new GameObject("Model label");
            labelObject.transform.SetParent(tile.transform);
            labelObject.transform.localPosition = new Vector3(-7, 0.03f, -6);
            labelObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = (i+1) + ". " + Path.GetFileName(Path.GetDirectoryName(models[i].path)) + "\n" +
                Path.GetFileName(models[i].path) + "\n" + models[i].observations + " -> " + models[i].continuous_actions;
            label.fontSize = 48; label.characterSize = 0.13f; label.color = Color.white;
            label.anchor = TextAnchor.UpperLeft;
        }
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera"; camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.farClipPlane = 500;
        camera.gameObject.AddComponent<AudioListener>();
        var center = new Vector3((columns-1)*9, 0, (rows-1)*9);
        camera.transform.position = center + new Vector3(0, Mathf.Max(columns,rows)*17, -Mathf.Max(columns,rows)*12);
        camera.transform.LookAt(center);
        var controls = new GameObject("Gallery controls").AddComponent<CandidateGalleryControls>();
        controls.galleryTitle = gallery.name; controls.caveat = gallery.caveat;
        controls.candidates = robots.ToArray(); controls.viewCamera = camera;
        controls.overviewPosition = camera.transform.position; controls.overviewRotation = camera.transform.eulerAngles;
        EditorSceneManager.SaveScene(scene, SceneFolder + "/" + gallery.name + ".unity");
    }

    static CandidateRobotAgent BuildRobot(Gallery gallery, ModelEntry model, Transform parent, Transform target)
    {
        string prefabPath = gallery.prefab;
        // Keep each historical experiment's physical prefab when one was supplied.
        if (gallery.contract == CandidateContract.Bipedal175)
        {
            var folder = Path.GetDirectoryName(model.path).Replace('\\', '/');
            var options = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            if (options.Length > 0) prefabPath = AssetDatabase.GUIDToAssetPath(options[0]);
        }
        else if (gallery.contract == CandidateContract.Bipedal191 && model.path.Contains("LBWSrEr_512/"))
            prefabPath = "Assets/AI/LBW/LBWSrEr_512/BipedalLegBase.prefab";
        else if (gallery.contract == CandidateContract.Forced113 && model.path.Contains("SQW04_FS_02/"))
            prefabPath = "Assets/AI/SQW04_FS_02/SymetryQudPed_FS_02.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) throw new InvalidOperationException("Prefab not found: " + prefabPath);
        var robot = UnityEngine.Object.Instantiate(prefab);
        robot.name = Path.GetFileNameWithoutExtension(model.path);
        robot.SetActive(false);
        robot.transform.SetParent(parent, false);
        var originalBipedal = robot.GetComponentInChildren<BipedalController>(true);
        var quad = robot.GetComponentInChildren<Agent.SymmetryQuadPed.SymmetryQuadPedController>(true);
        var forced = robot.GetComponentInChildren<Agent.SymmetryQuadPed.ForcedSymmetryQuadPedController>(true);
        ArticulationBody root;
        DriveController[] drives;
        Transform[] feet = Array.Empty<Transform>();
        if (originalBipedal != null)
        {
            root = originalBipedal.pelvis;
            drives = gallery.contract == CandidateContract.Bipedal191 ? new[] {
                originalBipedal.r_ThighX, originalBipedal.r_ThighZ, originalBipedal.r_ThighY,
                originalBipedal.r_Shine, originalBipedal.r_Foot, originalBipedal.l_ThighX,
                originalBipedal.l_ThighZ, originalBipedal.l_ThighY, originalBipedal.l_Shine,
                originalBipedal.l_Foot, originalBipedal.body } : originalBipedal.controllers.ToArray();
        }
        else if (forced != null)
        {
            root = forced.root;
            drives = new[] { forced.thighY1, forced.thighX1, forced.shine1, forced.thighY2, forced.thighX2,
                forced.shine2, forced.thighY3, forced.thighX3, forced.shine3, forced.thighY4, forced.thighX4, forced.shine4 };
            feet = new[] { forced.foot1.transform, forced.foot2.transform, forced.foot3.transform, forced.foot4.transform };
        }
        else if (quad != null)
        {
            root = quad.root;
            drives = new[] { quad.thighY1, quad.thighX1, quad.shine1, quad.thighY2, quad.thighX2, quad.shine2,
                quad.thighY3, quad.thighX3, quad.shine3, quad.thighY4, quad.thighX4, quad.shine4 };
            feet = new[] { quad.foot1.transform, quad.foot2.transform, quad.foot3.transform, quad.foot4.transform };
        }
        else throw new InvalidOperationException("No recognizable robot controller: " + prefabPath);
        if (drives.Any(d=>d==null)) throw new InvalidOperationException("Missing joint references: " + prefabPath);
        var bodies = drives.Select(d=>d.articulationBody != null ? d.articulationBody : d.GetComponent<ArticulationBody>()).ToList();
        var forces = drives.Select(d=>d.MaxForce > 0 ? d.MaxForce : 550).ToList();
        float budget = robot.GetComponentsInChildren<PowerHub>(true).Select(p=>p.UsableForce).DefaultIfEmpty(6050).First();
        if (budget <= 0) budget = forces.Sum();
        if (gallery.contract == CandidateContract.Quad126)
        {
            var historicalBodies = new List<ArticulationBody>();
            var historicalForces = new List<float>();
            for (int l = 0; l < 4; l++)
            {
                for (int j = 0; j < 3; j++) { historicalBodies.Add(bodies[l*3+j]); historicalForces.Add(forces[l*3+j]); }
                var foot = feet[l].GetComponent<ArticulationBody>();
                if (foot == null) throw new InvalidOperationException("Foot articulation missing: " + feet[l].name);
                foot.jointType = ArticulationJointType.RevoluteJoint;
                foot.twistLock = ArticulationDofLock.LimitedMotion;
                var drive = foot.xDrive;
                drive.lowerLimit = -90; drive.upperLimit = 90;
                drive.stiffness = bodies[l*3+2].xDrive.stiffness;
                drive.damping = bodies[l*3+2].xDrive.damping;
                drive.target = 0; drive.forceLimit = 0; foot.xDrive = drive;
                historicalBodies.Add(foot); historicalForces.Add(forces[l*3+2]);
            }
            bodies = historicalBodies; forces = historicalForces;
        }
        // Unpack into the test scene and remove only this copy's training scripts.
        foreach (var transform in robot.GetComponentsInChildren<Transform>(true))
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
        // Remove dependents first: Unity refuses to remove BehaviorParameters while
        // a required Agent is still attached, which would leave duplicate policies.
        foreach (var oldRequester in robot.GetComponentsInChildren<DecisionRequester>(true))
            UnityEngine.Object.DestroyImmediate(oldRequester);
        foreach (var originalAgent in robot.GetComponentsInChildren<Unity.MLAgents.Agent>(true))
            UnityEngine.Object.DestroyImmediate(originalAgent);
        foreach (var behaviour in robot.GetComponentsInChildren<MonoBehaviour>(true))
            UnityEngine.Object.DestroyImmediate(behaviour);
        foreach (var renderer in robot.GetComponentsInChildren<Renderer>(true)) renderer.sharedMaterial = robotMaterial;
        float lowest = robot.GetComponentsInChildren<Renderer>(true).Min(r=>r.bounds.min.y);
        robot.transform.position += Vector3.up * (0.05f - lowest);
        var agentObject = root.gameObject;
        var policy = agentObject.AddComponent<BehaviorParameters>();
        policy.BrainParameters.VectorObservationSize = model.observations;
        policy.BrainParameters.NumStackedVectorObservations = 1;
        policy.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(model.continuous_actions);
        policy.BehaviorName = gallery.name + "_" + Path.GetFileNameWithoutExtension(model.path);
        policy.BehaviorType = BehaviorType.InferenceOnly;
        policy.InferenceDevice = InferenceDevice.Burst;
        policy.DeterministicInference = true;
        policy.Model = AssetDatabase.LoadAssetAtPath<ModelAsset>(model.path);
        if (policy.Model == null) throw new InvalidOperationException("ONNX import failed: " + model.path);
        ModelLoader.Load(policy.Model); // Reject missing/broken Sentis assets during generation.
        var agent = agentObject.AddComponent<CandidateRobotAgent>();
        agent.contract = gallery.contract; agent.rootBody = root;
        agent.joints = bodies.ToArray(); agent.maximumJointForces = forces.ToArray();
        agent.feet = feet; agent.target = target; agent.availableForce = budget;
        agent.legOrder = gallery.oldLegOrder ? new[] {0,1,2,3} : new[] {3,0,1,2};
        agent.observationCount = model.observations; agent.actionCount = model.continuous_actions;
        agent.modelPath = model.path; agent.sourceCommit = gallery.commit;
        agent.reconstructedPhysics = gallery.contract == CandidateContract.Bipedal158 || gallery.contract == CandidateContract.Quad126;
        agent.MaxStep = 1500;
        var requester = agentObject.AddComponent<DecisionRequester>();
        requester.DecisionPeriod = 5; requester.TakeActionsBetweenDecisions = true;
        robot.SetActive(true);
        return agent;
    }
}
