using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
public static class InstallUrp {
    static AddRequest request;
    public static void Install() {
        request=Client.Add("com.unity.render-pipelines.universal");
        EditorApplication.update+=Poll;
    }
    static void Poll() {
        if(!request.IsCompleted)return;
        EditorApplication.update-=Poll;
        if(request.Status==StatusCode.Success){Debug.Log("URP_INSTALLED: "+request.Result.version);EditorApplication.Exit(0);}
        else {Debug.LogError(request.Error.message);EditorApplication.Exit(1);}
    }
}
