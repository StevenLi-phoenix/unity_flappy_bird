using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public static class BuildGame {
    static void Check(bool condition, string name) { if(!condition) throw new Exception("FAIL: "+name); Debug.Log("PASS: "+name); }
    public static void Build() { BuildFor(BuildTarget.StandaloneOSX,"Build/Flappy Bird.app"); }
    public static void BuildWebGL() { BuildFor(BuildTarget.WebGL,"Build/WebGL"); }
    static void BuildFor(BuildTarget target,string output) {
        FeedbackTests.Run();
        PresentationTests.Run();
        ResponsiveTests.Run();
        var playerSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        Check(playerSettings.FindProperty("activeInputHandler").intValue==1, "only the supported Input System is enabled");
        ConfigureRendering();
        Check(GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset, "URP is the default render pipeline");
        for(int i=0;i<QualitySettings.names.Length;i++)
            Check(QualitySettings.GetRenderPipelineAssetAt(i) == GraphicsSettings.defaultRenderPipeline, "URP quality level: "+QualitySettings.names[i]);
        Check(((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline).scriptableRenderer != null, "URP renderer is valid");
        Check(550+GameLayout.FirstPipeX*61-41 >= GameLayout.Width, "initial pipes start fully offscreen");
        Check(!GameLayout.PipeHasExited(-8.5f), "partially visible pipe is retained");
        Check(!GameLayout.PipeHasExited(-9.6f), "last visible pipe pixels are retained");
        Check(GameLayout.PipeHasExited(-9.7f), "fully hidden pipe can recycle");
        Check(550+GameLayout.NextPipeX(-20)*61-41 >= GameLayout.Width, "replacement always spawns offscreen");
        Check(Mathf.Approximately(GameLayout.NextPipeX(12),16), "normal pipe spacing is preserved");
        Check(GameLayout.PlayfieldTop == 0, "pipes meet sky boundary");
        Check(550+GameLayout.FirstPipeX*61-41 > 830, "first pipe clears title");
        Check(GameLayout.AnimationStep(true,1)==0 && GameLayout.AnimationStep(false,1)==1, "scenery freezes on game over");
        Check(!FlightRules.Hit(-2,0,-2,0),"bird fits gap");
        Check(FlightRules.Hit(-2,2,-2,0),"upper pipe collision");
        Check(FlightRules.Hit(-2,-2,-2,0),"lower pipe collision");
        Check(!FlightRules.Hit(-2,2,2,0),"distant pipe safe");
        Check(FlightRules.Bounds(-3.5f),"ground collision");
        Check(FlightRules.Bounds(6.2f),"ceiling collision");
        Check(!FlightRules.Bounds(0),"valid flight height");
        Check(FlightRules.Flap>0 && FlightRules.Gravity<0,"flight forces");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Flappy Bird").AddComponent<FlappyGame>();
        EditorSceneManager.SaveScene(scene,"Assets/FlappyBird.unity");
        PlayerSettings.productName="Flappy Bird";
        PlayerSettings.companyName="Pocket Arcade";
        PlayerSettings.defaultScreenWidth=1100; PlayerSettings.defaultScreenHeight=660;
        PlayerSettings.resizableWindow=true;
        Check(PlayerSettings.resizableWindow,"standalone window supports native resizing");
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        if(target==BuildTarget.WebGL){
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.template="PROJECT:Itch";
            Check(PlayerSettings.WebGL.decompressionFallback,"WebGL works without custom HTTP compression headers");
            Check(System.IO.File.Exists("Assets/WebGLTemplates/Itch/index.html"),"responsive WebGL template exists");
        }
        Debug.Log($"Building {target} to {output}");
        var report=BuildPipeline.BuildPlayer(new[]{"Assets/FlappyBird.unity"},output,target,BuildOptions.None);
        if(report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Build failed");
        Debug.Log("BUILD_AND_TESTS_PASSED");
    }
    static void ConfigureRendering() {
        const string folder="Assets/Settings";
        const string pipelinePath=folder+"/FlappyURP.asset";
        if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets","Settings");
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
        if(pipeline==null){
            var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer,folder+"/FlappyRenderer.asset");
            pipeline=UniversalRenderPipelineAsset.Create(renderer);
            pipeline.supportsHDR=false;
            pipeline.msaaSampleCount=1;
            AssetDatabase.CreateAsset(pipeline,pipelinePath);
        }
        GraphicsSettings.defaultRenderPipeline=pipeline;
        int previous=QualitySettings.GetQualityLevel();
        for(int i=0;i<QualitySettings.names.Length;i++){
            QualitySettings.SetQualityLevel(i,false);
            QualitySettings.renderPipeline=pipeline;
        }
        QualitySettings.SetQualityLevel(previous,false);
        AssetDatabase.SaveAssets();
        Debug.Log("Rendering configured: URP across all quality levels.");
    }
}
