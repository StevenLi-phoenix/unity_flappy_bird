using System;
using System.Collections;
using System.IO;
using UnityEngine;
// Opt-in rendered regression fixtures; never changes saved scores or settings.
public sealed class LayoutCapture : MonoBehaviour {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartIfRequested(){
#if !UNITY_WEBGL || UNITY_EDITOR
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--layout-check")>=0)new GameObject("Layout capture").AddComponent<LayoutCapture>();
#endif
    }
    IEnumerator Start(){
        Application.runInBackground=true;
        var game=FindAnyObjectByType<FlappyGame>();
        string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../LayoutChecks"));Directory.CreateDirectory(output);
        foreach(var size in new[]{new Vector2Int(550,330),new Vector2Int(1100,660),new Vector2Int(1906,1060),new Vector2Int(1600,720),new Vector2Int(600,1000)}){
            Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.6f);
            game.ShowLayoutCheck(true);
            yield return new WaitForEndOfFrame();
            var capture=ScreenCapture.CaptureScreenshotAsTexture();
            string name=$"result-{Screen.width}x{Screen.height}.png";File.WriteAllBytes(Path.Combine(output,name),capture.EncodeToPNG());Destroy(capture);
            Debug.Log("LAYOUT_CAPTURE: "+name);
        }
        Screen.SetResolution(1100,660,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(0.6f);
        game.ShowLayoutCheck(false);yield return new WaitForEndOfFrame();
        var start=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,"start.png"),start.EncodeToPNG());Destroy(start);
        Debug.Log("LAYOUT_CAPTURE_COMPLETE");Application.Quit();
    }
}
