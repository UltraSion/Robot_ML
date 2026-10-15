using System;
using System.IO;
using System.Linq;
using RobotML.Candidates;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CandidateVideoBatch
{
    [Serializable] public class Shot { public string scene; public int sceneNumber, modelNumber; }
    [Serializable] public class Config { public string outputDirectory, ffmpeg; public Shot[] shots; }
    const string Key = "RobotML.CandidateVideo.";
    static CandidateVideoBatch() { EditorApplication.update += Update; }
    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use the isolated recording project in batch mode.");
        SessionState.SetBool(Key+"running",true); SessionState.SetInt(Key+"index",0);
        SessionState.SetInt(Key+"phase",0); SessionState.SetBool(Key+"failed",false);
    }
    static void Update()
    {
        if (!SessionState.GetBool(Key+"running",false)) return;
        try { Tick(); }
        catch (Exception error) { Debug.LogException(error); SessionState.SetBool(Key+"running",false); EditorApplication.Exit(1); }
    }
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        var config = JsonUtility.FromJson<Config>(File.ReadAllText("capture-config.json"));
        int index = SessionState.GetInt(Key+"index",0), phase=SessionState.GetInt(Key+"phase",0);
        if (index >= config.shots.Length)
        {
            SessionState.SetBool(Key+"running",false);
            Debug.Log("ALL_CAPTURE_SHOTS_FINISHED");
            EditorApplication.Exit(SessionState.GetBool(Key+"failed",false) ? 1 : 0);
            return;
        }
        var shot = config.shots[index];
        if (phase == 0 && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.OpenScene(shot.scene);
            var controls = UnityEngine.Object.FindFirstObjectByType<CandidateGalleryControls>();
            var agent = controls.candidates[shot.modelNumber-1];
            foreach (var candidate in controls.candidates)
                if (candidate != agent) candidate.target.parent.gameObject.SetActive(false);
            controls.enabled=false;
            agent.targetSpeed=8; agent.MaxStep=100000; agent.episodeTravelRadius=14;
            agent.randomizeEpisodeHeading=true;
            var floor = agent.target.parent.Find("Floor");
            floor.localScale = new Vector3(32,0.5f,32);
            var label = agent.target.parent.Find("Model label"); if (label != null) label.gameObject.SetActive(false);
            ApplyGrid(floor.GetComponent<Renderer>());
            var driver = new GameObject("Candidate video capture").AddComponent<CandidateVideoCapture>();
            driver.robot=agent; driver.captureCamera=controls.viewCamera;
            driver.ffmpegPath=config.ffmpeg; driver.sceneNumber=shot.sceneNumber; driver.modelNumber=shot.modelNumber;
            driver.outputPath=Path.Combine(config.outputDirectory,$"Scene{shot.sceneNumber:00}_Model{shot.modelNumber:00}_speed8.mp4");
            SessionState.SetInt(Key+"phase",1);
            EditorApplication.EnterPlaymode();
        }
        else if (phase == 1 && EditorApplication.isPlaying)
        {
            var driver=UnityEngine.Object.FindFirstObjectByType<CandidateVideoCapture>();
            if (driver == null || !driver.finished) return;
            if (!driver.result.success) SessionState.SetBool(Key+"failed",true);
            SessionState.SetInt(Key+"phase",2);
            EditorApplication.ExitPlaymode();
        }
        else if (phase == 2 && !EditorApplication.isPlayingOrWillChangePlaymode)
        { SessionState.SetInt(Key+"index",index+1); SessionState.SetInt(Key+"phase",0); }
    }

    static void ApplyGrid(Renderer floor)
    {
        var texture = new Texture2D(64,64,TextureFormat.RGB24,false);
        for (int y=0;y<64;y++) for (int x=0;x<64;x++)
        {
            bool edge=x%16==0 || y%16==0;
            texture.SetPixel(x,y,edge ? new Color(0.22f,0.29f,0.36f) : ((x/16+y/16)%2==0 ? new Color(0.35f,0.42f,0.5f) : new Color(0.39f,0.46f,0.54f)));
        }
        texture.Apply(); texture.filterMode=FilterMode.Point; texture.wrapMode=TextureWrapMode.Repeat;
        var material = new Material(Shader.Find("Standard")); material.mainTexture=texture;
        material.mainTextureScale=new Vector2(8,8); floor.sharedMaterial=material;
    }
}
