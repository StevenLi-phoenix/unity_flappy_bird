using System;
using UnityEngine;
public static class FeedbackTests {
    static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);Debug.Log("PASS: "+name);}
    public static void Run(){
        foreach(FeedbackCue cue in Enum.GetValues(typeof(FeedbackCue))){
            var samples=ArcadeFeedback.Samples(cue);
            float peak=0,energy=0;
            foreach(float v in samples){CheckSample(v);peak=Mathf.Max(peak,Mathf.Abs(v));energy+=v*v;}
            Check(samples.Length>2000 && samples.Length<24000, cue+" duration is short");
            Check(energy>1 && peak<=0.45f,cue+" audio is audible and does not clip");
            Check(Mathf.Abs(samples[0])<0.001f && Mathf.Abs(samples[samples.Length-1])<0.001f,cue+" endpoints are click-free");
        }
        var fx=new FeedbackParticles();fx.Emit(FeedbackCue.Score,Vector2.zero);
        Check(fx.Items.Count>0,"score creates sparkles");
        fx.Tick(2);Check(fx.Items.Count==0,"particles expire");
        for(int i=0;i<100;i++)fx.Emit(FeedbackCue.Crash,Vector2.zero);
        Check(fx.Items.Count<=FeedbackParticles.Capacity,"particle count stays bounded");
        fx.Clear();Check(fx.Items.Count==0,"restart clears particles");
    }
    static void CheckSample(float v){if(float.IsNaN(v)||float.IsInfinity(v))throw new Exception("Nonfinite audio sample");}
}
