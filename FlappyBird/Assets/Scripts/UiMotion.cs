using UnityEngine;
public static class UiMotion {
    public static bool SoundInk(Vector2 p,bool muted){
        if(p.x>=12&&p.x<=21&&p.y>=26&&p.y<=38)return true;
        if(p.x>=20&&p.x<=32&&Mathf.Abs(p.y-32)<=6+(p.x-20)*0.75f)return true;
        if(muted){var d=p-new Vector2(44,32);return Mathf.Abs(d.x)<=6&&Mathf.Abs(d.y)<=6&&Mathf.Abs(Mathf.Abs(d.x)-Mathf.Abs(d.y))<=1.7f;}
        var v=p-new Vector2(28,32);float r=v.magnitude;
        return v.x>0&&Mathf.Abs(Mathf.Atan2(v.y,v.x))<0.68f&&(Mathf.Abs(r-15)<1.5f||Mathf.Abs(r-23)<1.5f);
    }
    public static Rect BestScoreRow(float textWidth,float centerY){float width=42+18+textWidth;return new Rect(550-width/2,centerY-22,width,44);}
    public static Matrix4x4 Around(Vector2 pivot,float angle,Vector2 scale)=>Matrix4x4.Translate(pivot)*Matrix4x4.TRS(Vector3.zero,Quaternion.Euler(0,0,angle),new Vector3(scale.x,scale.y,1))*Matrix4x4.Translate(-pivot);
    public static float Pop(float t){t=Mathf.Clamp01(t)-1;return 1+2.2f*t*t*t+1.2f*t*t;}
    public static float Fade(float current,float target,float dt)=>Mathf.MoveTowards(current,target,dt*5);
}
