using System;
using UnityEngine;
public static class ResponsiveTests {
    static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);Debug.Log("PASS: "+name);}
    public static void Run(){
        foreach(var resolution in new[]{new Vector2(550,330),new Vector2(1100,660),new Vector2(1906,1060),new Vector2(3248,1840),new Vector2(2560,1080),new Vector2(600,1000)}){
            var f=new ResponsiveLayout(resolution.x,resolution.y);string label=resolution.ToString();
            Check(Vector2.Distance(f.WorldToPixel(f.WorldBounds.min),Vector2.zero)<0.01f&&Vector2.Distance(f.WorldToPixel(f.WorldBounds.max),resolution)<0.01f,"world fills every edge "+label);
            Check(Mathf.Abs(f.WorldToPixel(new Vector2(0,660)).y-resolution.y)<0.01f,"ground stays at window bottom "+label);
            Check(Vector2.Distance(f.UiToPixel(new Vector2(550,330)),resolution/2)<0.01f,"UI centered "+label);
            var button=f.SoundButton.center;
            Check(Vector2.Distance(button,f.PixelToUi(f.UiToPixel(button)))<0.01f,"sound hitbox matches image "+label);
            float spawn=GameLayout.SpawnPipeX(f.WorldBounds.xMax);
            Check(550+spawn*61-41>f.WorldBounds.xMax,"pipes spawn outside actual window "+label);
            float atEdge=(f.WorldBounds.xMin-550-41)/61;
            Check(!GameLayout.PipeHasExited(atEdge+0.01f,f.WorldBounds.xMin)&&GameLayout.PipeHasExited(atEdge-0.01f,f.WorldBounds.xMin),"pipes recycle past actual window "+label);
        }
    }
}
