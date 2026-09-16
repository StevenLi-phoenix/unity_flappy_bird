using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class BuildGame {
    static void Check(bool condition, string name) { if(!condition) throw new Exception("FAIL: "+name); Debug.Log("PASS: "+name); }
    public static void Build() {
        var wide = GameLayout.Viewport(1600,780);
        var tall = GameLayout.Viewport(1100,1000);
        Check(wide == new Rect(250,0,1100,780), "wide window preserves aspect and centers");
        Check(tall == new Rect(0,110,1100,780), "tall window preserves aspect and centers");
        Check(GameLayout.PlayfieldTop == 90, "pipes meet sky boundary");
        Check(550+GameLayout.FirstPipeX*61-41 > 830, "first pipe clears title");
        Check(GameLayout.AnimationStep(true,1)==0 && GameLayout.AnimationStep(false,1)==1, "scenery freezes on game over");
        Check(!FlightRules.Hit(-2,0,-2,0),"bird fits gap");
        Check(FlightRules.Hit(-2,2,-2,0),"upper pipe collision");
        Check(FlightRules.Hit(-2,-2,-2,0),"lower pipe collision");
        Check(!FlightRules.Hit(-2,2,2,0),"distant pipe safe");
        Check(FlightRules.Bounds(-3.5f),"ground collision");
        Check(FlightRules.Bounds(5),"ceiling collision");
        Check(!FlightRules.Bounds(0),"valid flight height");
        Check(FlightRules.Flap>0 && FlightRules.Gravity<0,"flight forces");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Flappy Bird").AddComponent<FlappyGame>();
        EditorSceneManager.SaveScene(scene,"Assets/FlappyBird.unity");
        PlayerSettings.productName="Flappy Bird";
        PlayerSettings.companyName="Pocket Arcade";
        PlayerSettings.defaultScreenWidth=1100; PlayerSettings.defaultScreenHeight=780;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        var report=BuildPipeline.BuildPlayer(new[]{"Assets/FlappyBird.unity"},"Build/Flappy Bird.app",BuildTarget.StandaloneOSX,BuildOptions.None);
        if(report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Build failed");
        Debug.Log("BUILD_AND_TESTS_PASSED");
    }
}
