using UnityEngine;
using System.Collections.Generic;
using System.IO;
public class FlappyGame : MonoBehaviour {
    class Pipe { public float x,y; public bool scored; }
    readonly List<Pipe> pipes=new List<Pipe>();
    Texture2D pixel; GUIStyle text; float y,velocity,timer,worldTime; int score,best; bool playing,dead; int frames;
    Color sky=new Color32(113,199,205,255), ink=new Color32(39,66,63,255);
    void Awake(){
        Application.targetFrameRate=60;
        var camera=new GameObject("Camera").AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=sky;
        pixel=new Texture2D(1,1);pixel.SetPixel(0,0,Color.white);pixel.Apply();
        best=PlayerPrefs.GetInt("best",0); Reset(); Debug.Log("Flappy Bird ready. Space / click to flap.");
    }
    void Reset(){y=0.6f; velocity=0; score=0;timer=0;playing=false;dead=false;pipes.Clear(); for(int i=0;i<4;i++) pipes.Add(new Pipe{x=GameLayout.FirstPipeX+i*4.0f,y=i%2==0?0.35f:1.1f});}
    void Update(){
        float dt=Mathf.Min(Time.deltaTime,0.033f); worldTime+=GameLayout.AnimationStep(dead,dt);
        if(playing){
            velocity+=FlightRules.Gravity*dt;y+=velocity*dt;
            foreach(var p in pipes){p.x-=2.35f*dt;if(!p.scored&&p.x< -2.2f){p.scored=true;score++;Debug.Log("Score: "+score);} if(FlightRules.Hit(-2.2f,y,p.x,p.y)) Die();}
            if(pipes[0].x< -8.5f){pipes.RemoveAt(0);pipes.Add(new Pipe{x=pipes[pipes.Count-1].x+4,y=Random.Range(-0.1f,1.6f)});}
            if(FlightRules.Bounds(y)) Die();
        }else if(!dead)y=0.6f+Mathf.Sin(worldTime*3)*0.10f;
        timer+=dt;
        frames++;
        if(frames==100){var path=Path.Combine(Application.dataPath,"../../screenshot.png");ScreenCapture.CaptureScreenshot(path);Debug.Log("Screenshot requested: "+Path.GetFullPath(path));}
    }
    void Die(){if(dead)return;dead=true;playing=false;timer=0;best=Mathf.Max(best,score);PlayerPrefs.SetInt("best",best);PlayerPrefs.Save();Debug.Log("Game over. Score: "+score);}
    void Flap(){if(dead){if(timer<0.4f)return;Reset();}if(!playing){playing=true;Debug.Log("Flight started");}velocity=FlightRules.Flap;}
    void Box(float x,float yy,float w,float h,Color c){GUI.color=c;GUI.DrawTexture(new Rect(x,yy,w,h),pixel);GUI.color=Color.white;}
    Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
    void Label(string s,float x,float yy,float w,int size,Color color,TextAnchor align=TextAnchor.MiddleCenter){text.fontSize=size;text.alignment=align;text.normal.textColor=color;GUI.Label(new Rect(x,yy,w,size*1.6f),s,text);}
    void Shadow(string s,float x,float yy,float w,int size){Label(s,x+3,yy+4,w,size,ink);Label(s,x,yy,w,size,C("#fff8da"));}
    float PY(float value)=>390-value*61;
    void PipeDraw(Pipe p){float x=550+p.x*61; float top=PY(p.y+FlightRules.Gap/2),bottom=PY(p.y-FlightRules.Gap/2);
        float edge=GameLayout.PlayfieldTop;
        Box(x-34,edge,68,top-edge,ink);Box(x-30,edge,60,top-edge,C("#78b847"));Box(x-25,edge,12,top-edge,C("#b8e56b"));Box(x+18,edge,9,top-edge,C("#56923c"));
        Box(x-41,top-27,82,29,ink);Box(x-37,top-23,74,20,C("#93cd50"));Box(x-33,top-20,65,5,C("#c6eb78"));
        Box(x-34,bottom,68,600-bottom,ink);Box(x-30,bottom,60,600-bottom,C("#78b847"));Box(x-25,bottom,12,600-bottom,C("#b8e56b"));Box(x+18,bottom,9,600-bottom,C("#56923c"));Box(x-41,bottom,82,29,ink);Box(x-37,bottom+4,74,20,C("#93cd50"));Box(x-33,bottom+7,65,5,C("#c6eb78"));
    }
    void Bird(float x,float yy){
        Box(x-23,yy-18,39,36,ink);Box(x-29,yy-10,52,24,ink);Box(x-22,yy-14,37,28,C("#ffd651"));Box(x-19,yy-16,28,8,C("#fff19a"));Box(x-21,yy+9,34,7,C("#e5a431"));
        Box(x+4,yy-18,20,23,ink);Box(x+7,yy-15,14,17,Color.white);Box(x+16,yy-12,5,10,ink);
        Box(x+13,yy+3,28,14,ink);Box(x+17,yy+5,23,4,C("#f88543"));Box(x+17,yy+11,19,3,C("#e76635"));
        float wing=Mathf.Sin(worldTime*15)>0?0:5;Box(x-30,yy+wing-3,24,17,ink);Box(x-26,yy+wing,18,10,C("#fff0a2"));
    }
    void OnGUI(){
        if(pixel==null)return;
        GUI.matrix=Matrix4x4.identity;
        Box(0,0,Screen.width,Screen.height,ink);
        Rect viewport=GameLayout.Viewport(Screen.width,Screen.height);
        float scale=viewport.width/GameLayout.Width;
        GUI.BeginGroup(viewport);
        GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
        if(text==null)text=new GUIStyle(GUI.skin.label){fontStyle=FontStyle.Bold};
        var e=Event.current;if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Space){Flap();e.Use();}if(e.type==EventType.MouseDown&&e.button==0){Flap();e.Use();}
        Box(0,0,1100,780,C("#f8f3df"));Box(0,0,1100,90,ink);
        Label("POCKET / ARCADE",35,22,400,24,C("#fff8da"),TextAnchor.MiddleLeft);Label("01  —  FLAPPY BIRD",665,25,395,18,C("#bce2c8"),TextAnchor.MiddleRight);
        Box(0,90,1100,526,sky);
        // Stepped clouds and a distant skyline.
        for(int i=0;i<7;i++){float x=i*185-40;Box(x,445,150,53,C("#d7efda"));Box(x+20,423,100,35,C("#d7efda"));Box(x+45,408,48,28,C("#d7efda"));}
        for(int i=0;i<20;i++){float x=i*62;float h=35+(i*31%60);Box(x,535-h,48,h,C("#add9bb"));for(int j=0;j<3;j++)Box(x+8+j*12,544-h,5,12,C("#d2edc9"));}
        for(int i=0;i<30;i++){float x=i*41;Box(x,552,43,49,C("#89bf82"));Box(x+6,539+(i%3)*4,30,20,C("#89bf82"));}
        foreach(var p in pipes)PipeDraw(p);
        Bird(550-2.2f*61,PY(y));
        Box(0,600,1100,5,ink);Box(0,605,1100,18,C("#c1df73"));for(int i=0;i<60;i++)Box(i*24-(worldTime*40%24),610,12,8,C("#84b957"));Box(0,623,1100,5,C("#8e9b52"));Box(0,628,1100,49,C("#e9d69a"));
        for(int i=0;i<60;i++)Box(i*29,638+(i%3)*10,4,3,C("#d2bc7e"));
        if(!playing&&!dead){Shadow("FLAPPY BIRD",155,135,790,64);Label("A LITTLE BIRD. A BIG ADVENTURE.",240,222,620,16,ink);Box(302,477,496,67,ink);Box(307,482,486,57,C("#fff8da"));Label("SPACE / CLICK TO FLY",307,489,486,23,ink);}
        if(playing)Shadow(score.ToString(),450,125,200,60);
        if(dead){Box(325,215,450,270,ink);Box(331,221,438,258,C("#fff8da"));Label("GAME OVER",340,237,420,42,ink);Label("SCORE  "+score+"     BEST  "+best,350,312,400,25,ink);Label("SPACE / CLICK TO RETRY",340,393,420,20,ink);}
        Label("SPACE / CLICK",36,700,260,20,ink,TextAnchor.MiddleLeft);Label("Flap. Find the gap. Repeat.",350,701,400,18,C("#718575"));Label("BEST  "+best.ToString("00"),835,700,225,20,ink,TextAnchor.MiddleRight);
        Label("ONE MORE TRY?",35,740,500,11,C("#718575"),TextAnchor.MiddleLeft);
        GUI.matrix=Matrix4x4.identity;
        GUI.EndGroup();
    }
}
