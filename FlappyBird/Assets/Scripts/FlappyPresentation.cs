using UnityEngine;
using System.Collections.Generic;
public partial class FlappyGame {
    readonly Dictionary<string,Texture2D> roundTextures=new Dictionary<string,Texture2D>();
    Rect SoundRect=>layout.SoundButton;
    void Rotate(float angle,Vector2 pivot){drawMatrix=drawMatrix*UiMotion.Around(pivot,angle,Vector2.one);}
    void Scale(Vector2 scale,Vector2 pivot){drawMatrix=drawMatrix*UiMotion.Around(pivot,0,scale);}
    void Round(float x,float y,float w,float h,float radius,Color color){
        int iw=Mathf.CeilToInt(w),ih=Mathf.CeilToInt(h);string key=iw+":"+ih+":"+radius;
        if(!roundTextures.TryGetValue(key,out var texture)){
            texture=new Texture2D(iw,ih,TextureFormat.RGBA32,false);texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;
            var pixels=new Color[iw*ih];float r=Mathf.Min(radius,Mathf.Min(w,h)/2);
            for(int py=0;py<ih;py++)for(int px=0;px<iw;px++){
                float dx=Mathf.Max(Mathf.Abs(px+0.5f-w/2)-(w/2-r),0),dy=Mathf.Max(Mathf.Abs(py+0.5f-h/2)-(h/2-r),0);
                pixels[py*iw+px]=new Color(1,1,1,Mathf.Clamp01(r-Mathf.Sqrt(dx*dx+dy*dy)+0.5f));
            }
            texture.SetPixels(pixels);texture.Apply();roundTextures.Add(key,texture);
        }
        color.a*=uiAlpha;painter.Image(texture,new Rect(x,y,w,h),color,drawMatrix);
    }
    void Bird(float x,float y){
        Round(x-32,y-23,64,48,23,ink);
        Round(x-28,y-20,56,40,20,C("#ffd655"));
        Round(x-20,y-17,29,10,5,C("#fff2a2"));
        Round(x+8,y-22,25,28,12,ink);Round(x+11,y-19,20,23,10,Color.white);
        Round(x+23,y-13,6,11,3,ink);
        Round(x+14,y+3,33,17,8,ink);Round(x+17,y+6,27,10,5,C("#f88749"));
        float wing=Mathf.Sin(worldTime*16)*5;
        Round(x-36,y-2+wing,29,23,11,ink);Round(x-32,y+1+wing,22,16,8,C("#fff0ad"));
    }
    void Triangle(float x,float y,float size,Color color){
        string key="triangle:"+size;
        if(!roundTextures.TryGetValue(key,out var texture)){
            int n=Mathf.CeilToInt(size);texture=new Texture2D(n,n,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
            var data=new Color[n*n];
            for(int py=0;py<n;py++)for(int px=0;px<n;px++)data[py*n+px]=new Color(1,1,1,Mathf.Clamp01((n-px)/2f-Mathf.Abs(py+0.5f-n/2f)));
            texture.SetPixels(data);texture.Apply();roundTextures.Add(key,texture);
        }
        color.a*=uiAlpha;painter.Image(texture,new Rect(x,y,size,size),color,drawMatrix);
    }
    void Replay(float x,float y,Color color){
        for(int i=0;i<24;i++){float angle=(i*11+40)*Mathf.Deg2Rad;Round(x+Mathf.Cos(angle)*15-3,y+Mathf.Sin(angle)*15-3,7,7,3.5f,color);}
        var matrix=drawMatrix;Rotate(150,new Vector2(x+12,y-12));Triangle(x+5,y-19,16,color);drawMatrix=matrix;
    }
    void Trophy(float x,float y,Color color){
        Round(x-13,y-14,26,23,6,color);Round(x-4,y+6,8,13,3,color);Round(x-13,y+17,26,5,2,color);
        Round(x-21,y-10,10,15,5,color);Round(x+11,y-10,10,15,5,color);
    }
    void ActionButton(Rect rect,bool replay){
        var matrix=drawMatrix;float hover=rect.Contains(pointer)?(pointerHeld?0.95f:1.05f):1;
        Scale(new Vector2(hover,hover),rect.center);
        Round(rect.x,rect.y+7,rect.width,rect.height,rect.height/2,C("#368f79"));
        Round(rect.x,rect.y,rect.width,rect.height,rect.height/2,C("#fff8df"));
        if(replay)Replay(rect.center.x,rect.center.y,ink);
        else Triangle(rect.center.x-9,rect.center.y-16,32,ink);
        drawMatrix=matrix;
    }
    void SoundButton(){
        var r=SoundRect;
        Round(r.x,r.y,r.width,r.height,32,C("#fff8df"));
        string key=feedback.Muted?"speaker-off":"speaker-on";
        if(!roundTextures.TryGetValue(key,out var texture)){
            texture=new Texture2D(128,128,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
            var data=new Color[128*128];
            for(int py=0;py<128;py++)for(int px=0;px<128;px++){
                float alpha=0;
                for(int sy=0;sy<2;sy++)for(int sx=0;sx<2;sx++)if(UiMotion.SoundInk(new Vector2((px+(sx+0.5f)/2)/2,(py+(sy+0.5f)/2)/2),feedback.Muted))alpha+=0.25f;
                data[py*128+px]=new Color(1,1,1,alpha);
            }
            texture.SetPixels(data);texture.Apply();roundTextures.Add(key,texture);
        }
        painter.Image(texture,r,ink,drawMatrix);
    }
    void DrawIntro(){
        if(introAlpha<=0)return;
        uiAlpha=introAlpha;
        var matrix=drawMatrix;float pop=0.88f+0.12f*UiMotion.Pop(introAge/0.6f);
        Scale(new Vector2(pop,pop),new Vector2(550,210));
        float drift=Mathf.Sin(worldTime*1.5f)*4;
        Shadow("Flappy Bird",200,115+drift,700,76);
        drawMatrix=matrix;
        float bounce=Mathf.Sin(worldTime*3)*4;
        ActionButton(new Rect(491,430+bounce,118,78),false);
        // A breathing space-key glyph demonstrates the alternative keyboard input.
        Round(514,535,72,26,8,new Color(1,1,0.94f,0.55f));
        Box(529,546,3,6,ink);Box(529,550,42,3,ink);Box(568,546,3,6,ink);
        uiAlpha=1;
    }
    void DrawEnd(){
        if(endAlpha<=0)return;
        uiAlpha=endAlpha;
        var matrix=drawMatrix;float reveal=UiMotion.Pop(Mathf.Max(0,endAge-0.18f)/0.48f);
        float s=0.82f+0.18f*reveal;
        Scale(new Vector2(s,s),new Vector2(550,320));
        float y=155+(1-reveal)*35;
        Round(378,y+9,344,316,34,new Color(0.12f,0.31f,0.29f,0.20f));
        Round(378,y,344,316,34,C("#fff8df"));
        Label("Nice flight!",395,y+19,310,25,C("#648779"));
        Label(lastScore.ToString(),395,y+49,310,80,ink);
        string bestText=best.ToString();
        var bestRow=UiMotion.BestScoreRow(painter.TextWidth(bestText,27),y+190);
        Trophy(bestRow.x+21,bestRow.center.y-4,C("#e8ae38"));
        painter.Label(bestText,new Rect(bestRow.x+60,bestRow.y,bestRow.width-60,bestRow.height),27,new Color(ink.r,ink.g,ink.b,uiAlpha),TextAnchor.MiddleLeft,drawMatrix);
        ActionButton(new Rect(499,y+232,102,62),true);
        drawMatrix=matrix;uiAlpha=1;
    }
    void LateUpdate(){
        if(pixel==null)return;
        painter.Begin();uiAlpha=1;drawMatrix=Matrix4x4.identity;
        Box(0,0,layout.Width,layout.Height,sky);
        drawMatrix=Matrix4x4.Translate(layout.WorldOrigin);
        float left=layout.WorldBounds.xMin,right=layout.WorldBounds.xMax;
        // Soft clouds, with the familiar distant pixel skyline underneath.
        for(int i=Mathf.FloorToInt(left/185)-1;i*185-40<right;i++){float x=i*185-40;Round(x,445,150,53,26,C("#d7efda"));Round(x+20,423,100,55,27,C("#d7efda"));Round(x+45,408,48,54,24,C("#d7efda"));}
        for(int i=Mathf.FloorToInt(left/62)-1;i*62<right;i++){float x=i*62;float h=35+Mathf.Abs(i*31%60);Box(x,535-h,48,h,C("#add9bb"));for(int j=0;j<3;j++)Box(x+8+j*12,544-h,5,12,C("#d2edc9"));}
        for(int i=Mathf.FloorToInt(left/41)-1;i*41<right;i++){float x=i*41;Round(x,550,46,55,18,C("#89bf82"));Round(x+6,539+Mathf.Abs(i%3)*4,30,35,15,C("#89bf82"));}
        foreach(var p in pipes)PipeDraw(p);
        foreach(var p in particles.Items){var color=p.Color;color.a=p.Life/p.Duration;Box(p.Position.x,p.Position.y,p.Size,p.Size,color);}
        var birdMatrix=drawMatrix;
        Rotate(playing?Mathf.Clamp(-velocity*4,-22,65):dead?65:0,new Vector2(550-2.2f*61,PY(y)));
        Bird(550-2.2f*61,PY(y));drawMatrix=birdMatrix;
        Box(left,600,layout.Width,5,ink);Box(left,605,layout.Width,18,C("#c1df73"));for(int i=Mathf.FloorToInt(left/24)-1;i*24<right+24;i++)Box(i*24-(worldTime*40%24),610,12,8,C("#84b957"));Box(left,623,layout.Width,5,C("#8e9b52"));Box(left,628,layout.Width,32,C("#e9d69a"));
        for(int i=Mathf.FloorToInt(left/29)-1;i*29<right;i++)Box(i*29,638+Mathf.Abs(i%2)*10,4,3,C("#d2bc7e"));
        drawMatrix=Matrix4x4.identity;
        if(impact>0)Box(0,0,layout.Width,layout.Height,new Color(1,0.94f,0.75f,impact*1.3f));
        drawMatrix=Matrix4x4.Translate(layout.UiOrigin);
        if(playing){uiAlpha=1-introAlpha;Shadow(score.ToString(),450,33,200,60+Mathf.RoundToInt(scorePulse*16));uiAlpha=1;}
        if(scorePulse>0)Label("+1",450,130+(0.65f-scorePulse)*-30,200,24,new Color(1,0.98f,0.8f,scorePulse/0.65f));
        DrawIntro();DrawEnd();SoundButton();
        painter.End();
    }
    void OnDestroy(){foreach(var texture in roundTextures.Values)Destroy(texture);if(pixel!=null)Destroy(pixel);}
}
