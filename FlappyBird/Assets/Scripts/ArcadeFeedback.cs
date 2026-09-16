using UnityEngine;
using System.Collections.Generic;
public enum FeedbackCue { Flap, Score, Crash }
public sealed class ArcadeFeedback : MonoBehaviour {
    public const int SampleRate=44100;
    AudioSource source; readonly AudioClip[] clips=new AudioClip[3];
    public bool Muted { get; private set; }
    void Awake(){
        source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=0.65f;
        Muted=PlayerPrefs.GetInt("muted",0)!=0;source.mute=Muted;
        for(int i=0;i<clips.Length;i++){var data=Samples((FeedbackCue)i);clips[i]=AudioClip.Create(((FeedbackCue)i).ToString(),data.Length,1,SampleRate,false);clips[i].SetData(data,0);}
        Debug.Log("Feedback ready: synthesized flap, score and crash sounds. M toggles sound.");
    }
    public void Play(FeedbackCue cue){if(cue==FeedbackCue.Crash)source.Stop();source.PlayOneShot(clips[(int)cue]);}
    public void ToggleMute(){Muted=!Muted;source.mute=Muted;PlayerPrefs.SetInt("muted",Muted?1:0);PlayerPrefs.Save();Debug.Log("Sound muted: "+Muted);}
    void OnDestroy(){foreach(var clip in clips)if(clip!=null)Destroy(clip);}
    public static float[] Samples(FeedbackCue cue){
        float duration=cue==FeedbackCue.Flap?0.12f:cue==FeedbackCue.Score?0.30f:0.34f;
        var data=new float[Mathf.RoundToInt(duration*SampleRate)];double phase=0;var noise=new System.Random(23);
        for(int i=0;i<data.Length;i++){
            float t=i/(float)SampleRate,u=i/(float)(data.Length-1);
            float frequency=cue==FeedbackCue.Flap?Mathf.Lerp(850,330,u):cue==FeedbackCue.Score?(t<0.12f?880:1320):Mathf.Lerp(170,45,u);
            phase+=2*System.Math.PI*frequency/SampleRate;
            float tone=(float)System.Math.Sin(phase);
            if(cue==FeedbackCue.Crash)tone=0.65f*tone+0.35f*((float)noise.NextDouble()*2-1);
            float envelope=Mathf.Min(1,t/0.006f)*Mathf.Pow(1-u,1.5f);
            if(cue==FeedbackCue.Score)envelope*=Mathf.Clamp01(Mathf.Abs(t-0.12f)/0.005f);
            data[i]=tone*envelope*0.4f;
        }
        return data;
    }
}
public sealed class FeedbackParticles {
    public const int Capacity=96;
    public struct Particle { public Vector2 Position,Velocity; public float Life,Duration,Size; public Color Color; }
    public readonly List<Particle> Items=new List<Particle>();
    public void Emit(FeedbackCue cue,Vector2 origin){
        int count=cue==FeedbackCue.Flap?6:cue==FeedbackCue.Score?20:26;
        for(int i=0;i<count;i++){
            if(Items.Count>=Capacity)Items.RemoveAt(0);
            float angle=i*2.39996f,speed=45+(i%5)*23,duration=cue==FeedbackCue.Flap?0.3f:0.65f;
            var velocity=cue==FeedbackCue.Flap?new Vector2(-90-i*13,20+(i-3)*15):new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*speed;
            Items.Add(new Particle{Position=origin,Velocity=velocity,Life=duration,Duration=duration,Size=cue==FeedbackCue.Flap?5:4+i%4,Color=cue==FeedbackCue.Crash?new Color(1,0.67f,0.25f):new Color(1,0.96f,0.68f)});
        }
    }
    public void Tick(float dt){for(int i=Items.Count-1;i>=0;i--){var p=Items[i];p.Life-=dt;if(p.Life<=0){Items.RemoveAt(i);continue;}p.Position+=p.Velocity*dt;p.Velocity.y+=110*dt;Items[i]=p;}}
    public void Clear()=>Items.Clear();
}
