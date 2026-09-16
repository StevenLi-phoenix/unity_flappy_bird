using System;
using UnityEngine;
public static class PresentationTests {
    static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);Debug.Log("PASS: "+name);}
    public static void Run(){
        Check(UiMotion.SoundInk(new Vector2(15,32),false),"speaker has a rectangular driver");
        Check(UiMotion.SoundInk(new Vector2(30,20),false),"speaker cone opens toward the waves");
        Check(UiMotion.SoundInk(new Vector2(51,32),false),"sound-on icon shows outer wave");
        Check(!UiMotion.SoundInk(new Vector2(51,32),true),"muted icon removes sound waves");
        Check(UiMotion.SoundInk(new Vector2(44,32),true),"muted icon has a separate cross");
        foreach(float width in new[]{18f,36f,54f}){
            var row=UiMotion.BestScoreRow(width,340);
            Check(Mathf.Approximately(row.center.x,550),"trophy row stays centered for text width "+width);
            Check(Mathf.Approximately(row.width-42-width,18),"trophy keeps 18px gap for text width "+width);
        }
        var pivot=new Vector2(415.8f,353.4f);
        foreach(float scale in new[]{0.63f,1.34f}){
            var canvas=Matrix4x4.TRS(new Vector3(120,45,0),Quaternion.identity,new Vector3(scale,scale,1));
            var before=canvas.MultiplyPoint3x4(pivot);
            var after=(canvas*UiMotion.Around(pivot,-22,Vector2.one)).MultiplyPoint3x4(pivot);
            Check(Vector3.Distance(before,after)<0.001f,"flap rotation cannot teleport bird at scale "+scale);
        }
        Check(Mathf.Approximately(UiMotion.Pop(0),0)&&Mathf.Approximately(UiMotion.Pop(1),1),"pop animation has stable endpoints");
        Check(UiMotion.Pop(0.7f)>1,"pop animation has a soft overshoot");
        Check(UiMotion.Fade(0,1,0.1f)>0 && UiMotion.Fade(0,1,0.1f)<1,"entrance fades over time");
        Check(UiMotion.Fade(1,0,0.1f)>0 && UiMotion.Fade(1,0,0.1f)<1,"exit fades over time");
        Check(UiMotion.Fade(1,0,1)==0,"exit animation finishes");
        var view=new ResponsiveLayout(1400,900);
        Check(Vector2.Distance(view.PixelToUi(new Vector2(700,450)),new Vector2(GameLayout.Width/2,GameLayout.Height/2))<0.001f,"resized pointer maps to canvas center");
        Check(GameLayout.PlayfieldTop==0,"sky reaches the top without a text bar");
        Check(GameLayout.Height==660,"bottom ends in ground without a text bar");
    }
}
