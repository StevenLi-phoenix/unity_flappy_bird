using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;
public partial class FlappyGame : MonoBehaviour {
    class Pipe { public float x,y; public bool scored; }
    readonly List<Pipe> pipes=new List<Pipe>();
    readonly FeedbackParticles particles=new FeedbackParticles();
    ArcadeFeedback feedback; float impact,scorePulse;
    float introAlpha=1,endAlpha,introAge,endAge,uiAlpha=1; int lastScore;
    Vector2 pointer; bool pointerHeld;
    CanvasPainter painter; ResponsiveLayout layout; Matrix4x4 drawMatrix=Matrix4x4.identity;
    int viewWidth,viewHeight;
    bool layoutCheckActive;
    internal void ShowLayoutCheck(bool result){layoutCheckActive=true;playing=false;dead=result;lastScore=0;best=25;introAlpha=result?0:1;endAlpha=result?1:0;endAge=1;introAge=1;impact=0;scorePulse=0;y=0.6f;}
    Texture2D pixel; float y,velocity,timer,worldTime; int score,best; bool playing,dead;
#if !UNITY_WEBGL || UNITY_EDITOR
    int frames;
#endif
    Color sky=new Color32(113,199,205,255), ink=new Color32(39,66,63,255);
    void Awake(){
        Application.targetFrameRate=60;
        var camera=new GameObject("Camera").AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=sky;
        camera.gameObject.AddComponent<AudioListener>();
        feedback=gameObject.AddComponent<ArcadeFeedback>();
        painter=gameObject.AddComponent<CanvasPainter>();painter.Initialize();
        layout=new ResponsiveLayout(Screen.width,Screen.height);
        pixel=new Texture2D(1,1);pixel.SetPixel(0,0,Color.white);pixel.Apply();
        best=PlayerPrefs.GetInt("best",0); Reset(); Debug.Log("Flappy Bird ready. Space / click to flap.");
    }
    void Reset(){particles.Clear();impact=0;scorePulse=0;y=0.6f; velocity=0; score=0;timer=0;playing=false;dead=false;pipes.Clear(); for(int i=0;i<4;i++) pipes.Add(new Pipe{x=GameLayout.SpawnPipeX(layout.WorldBounds.xMax)+i*GameLayout.PipeSpacing,y=i%2==0?0.35f:1.1f});}
    void Update(){
        layout=new ResponsiveLayout(Screen.width,Screen.height);
        if(viewWidth!=Screen.width||viewHeight!=Screen.height){
            viewWidth=Screen.width;viewHeight=Screen.height;
            if(!playing&&!dead)Reset();
            Debug.Log($"Viewport {viewWidth}x{viewHeight}: canvas={layout.Width:F1}x{layout.Height:F1}, scale={layout.Scale:F3}");
        }
        if(layoutCheckActive)return;
        if(Mouse.current!=null){var pos=Mouse.current.position.ReadValue();pointer=layout.PixelToUi(new Vector2(pos.x,Screen.height-pos.y));pointerHeld=Mouse.current.leftButton.isPressed;}
        introAge+=Time.deltaTime;
        introAlpha=UiMotion.Fade(introAlpha,!playing&&!dead?1:0,Time.deltaTime);
        endAlpha=UiMotion.Fade(endAlpha,dead&&timer>0.18f?1:0,Time.deltaTime);
        if(dead)endAge+=Time.deltaTime;
        if(Keyboard.current!=null && Keyboard.current.mKey.wasPressedThisFrame)feedback.ToggleMute();
        particles.Tick(Time.deltaTime);impact=Mathf.Max(0,impact-Time.deltaTime);scorePulse=Mathf.Max(0,scorePulse-Time.deltaTime);
        bool click=Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame;
        if(click && SoundRect.Contains(pointer))feedback.ToggleMute();
        else if((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
           (click && new Rect(-layout.UiOrigin, new Vector2(layout.Width,layout.Height)).Contains(pointer))) Flap();
        float dt=Mathf.Min(Time.deltaTime,0.033f); worldTime+=GameLayout.AnimationStep(dead,dt);
        if(playing){
            velocity+=FlightRules.Gravity*dt;y+=velocity*dt;
            foreach(var p in pipes){p.x-=2.35f*dt;if(FlightRules.Hit(-2.2f,y,p.x,p.y)){Die();break;}if(!p.scored&&p.x< -2.2f-(GameLayout.PipeHalfWidth+30)/GameLayout.PixelsPerUnit){p.scored=true;score++;scorePulse=0.65f;feedback.Play(FeedbackCue.Score);particles.Emit(FeedbackCue.Score,new Vector2(550-2.2f*61,PY(y)));Debug.Log("Score: "+score);}}
            if(GameLayout.PipeHasExited(pipes[0].x,layout.WorldBounds.xMin)){
                float removedX=pipes[0].x;
                pipes.RemoveAt(0);
                float spawnX=GameLayout.NextPipeX(pipes[pipes.Count-1].x,layout.WorldBounds.xMax);
                pipes.Add(new Pipe{x=spawnX,y=Random.Range(-0.1f,1.6f)});
                Debug.Log($"Pipe recycled outside viewport: removed x={removedX:F2}, spawned x={spawnX:F2}");
            }
            while(pipes[pipes.Count-1].x<GameLayout.SpawnPipeX(layout.WorldBounds.xMax)+GameLayout.PipeSpacing)
                pipes.Add(new Pipe{x=pipes[pipes.Count-1].x+GameLayout.PipeSpacing,y=Random.Range(-0.1f,1.6f)});
            if(FlightRules.Bounds(y,layout.WorldBounds.yMin)) Die();
        }else if(!dead)y=0.6f+Mathf.Sin(worldTime*3)*0.10f;
        timer+=dt;
#if !UNITY_WEBGL || UNITY_EDITOR
        frames++;
        if(frames==100){var path=Path.GetFullPath(Path.Combine(Application.dataPath,Application.isEditor?"../Build/screenshot.png":"../../screenshot.png"));Directory.CreateDirectory(Path.GetDirectoryName(path));ScreenCapture.CaptureScreenshot(path);Debug.Log("Screenshot requested: "+path);}
#endif
    }
    void Die(){if(dead)return;dead=true;playing=false;timer=0;endAge=0;lastScore=score;impact=0.24f;feedback.Play(FeedbackCue.Crash);particles.Emit(FeedbackCue.Crash,new Vector2(550-2.2f*61,PY(y)));best=Mathf.Max(best,score);PlayerPrefs.SetInt("best",best);PlayerPrefs.Save();Debug.Log("Game over. Score: "+score);}
    void Flap(){if(dead){if(timer<0.4f)return;Reset();}if(!playing){playing=true;Debug.Log("Flight started");}velocity=FlightRules.Flap;feedback.Play(FeedbackCue.Flap);particles.Emit(FeedbackCue.Flap,new Vector2(550-2.2f*61-20,PY(y)+8));}
    void Box(float x,float yy,float w,float h,Color c){c.a*=uiAlpha;painter.Image(pixel,new Rect(x,yy,w,h),c,drawMatrix);}
    Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
    void Label(string s,float x,float yy,float w,int size,Color color,TextAnchor align=TextAnchor.MiddleCenter){color.a*=uiAlpha;painter.Label(s,new Rect(x,yy,w,size*1.6f),size,color,align,drawMatrix);}
    void Shadow(string s,float x,float yy,float w,int size){Label(s,x+3,yy+4,w,size,ink);Label(s,x,yy,w,size,C("#fff8da"));}
    float PY(float value)=>390-value*61;
    void PipeDraw(Pipe p){float x=GameLayout.Width/2+p.x*GameLayout.PixelsPerUnit; float top=PY(p.y+FlightRules.Gap/2),bottom=PY(p.y-FlightRules.Gap/2);
        float edge=layout.WorldBounds.yMin;
        Box(x-34,edge,68,top-edge,ink);Box(x-30,edge,60,top-edge,C("#78b847"));Box(x-25,edge,12,top-edge,C("#b8e56b"));Box(x+18,edge,9,top-edge,C("#56923c"));
        Box(x-41,top-27,82,29,ink);Box(x-37,top-23,74,20,C("#93cd50"));Box(x-33,top-20,65,5,C("#c6eb78"));
        Box(x-34,bottom,68,600-bottom,ink);Box(x-30,bottom,60,600-bottom,C("#78b847"));Box(x-25,bottom,12,600-bottom,C("#b8e56b"));Box(x+18,bottom,9,600-bottom,C("#56923c"));Box(x-41,bottom,82,29,ink);Box(x-37,bottom+4,74,20,C("#93cd50"));Box(x-33,bottom+7,65,5,C("#c6eb78"));
    }
}
