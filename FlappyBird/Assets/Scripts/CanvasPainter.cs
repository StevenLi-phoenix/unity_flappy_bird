using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Retained, pooled Canvas graphics. No IMGUI groups, font layout, or GUI matrices.
public sealed class CanvasPainter : MonoBehaviour {
    sealed class Slot { public GameObject Root; public RawImage Image; public Text Text; }
    readonly List<Slot> slots=new List<Slot>();
    readonly TextGenerator measure=new TextGenerator();
    RectTransform root;Font font;int used;
    public void Initialize(){
        var go=new GameObject("Game Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        go.transform.SetParent(transform,false);root=(RectTransform)go.transform;
        var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10;
        var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(GameLayout.Width,GameLayout.Height);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Debug.Log("Canvas renderer ready: shared reference scale, centered UI, bottom-anchored world.");
    }
    public void Begin(){used=0;}
    public void End(){for(int i=used;i<slots.Count;i++)if(slots[i].Root.activeSelf)slots[i].Root.SetActive(false);}
    Slot Next(bool label){
        if(used==slots.Count){
            var go=new GameObject("Draw "+used,typeof(RectTransform));go.transform.SetParent(root,false);
            var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=Vector2.zero;rt.sizeDelta=Vector2.zero;
            var imageGo=new GameObject("Image",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));imageGo.transform.SetParent(go.transform,false);
            var textGo=new GameObject("Text",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));textGo.transform.SetParent(go.transform,false);
            var s=new Slot{Root=go,Image=imageGo.GetComponent<RawImage>(),Text=textGo.GetComponent<Text>()};
            s.Image.raycastTarget=false;s.Text.raycastTarget=false;s.Text.font=font;s.Text.fontStyle=FontStyle.Bold;s.Text.supportRichText=false;s.Text.alignByGeometry=true;
            s.Text.horizontalOverflow=HorizontalWrapMode.Overflow;s.Text.verticalOverflow=VerticalWrapMode.Overflow;
            slots.Add(s);
        }
        var slot=slots[used++];if(!slot.Root.activeSelf)slot.Root.SetActive(true);
        if(slot.Text.gameObject.activeSelf!=label)slot.Text.gameObject.SetActive(label);
        if(slot.Image.gameObject.activeSelf==label)slot.Image.gameObject.SetActive(!label);
        return slot;
    }
    static void Place(RectTransform rt,Rect rect,Matrix4x4 matrix){
        Vector3 center=matrix.MultiplyPoint3x4(rect.center);
        float sx=new Vector2(matrix.m00,matrix.m10).magnitude,sy=new Vector2(matrix.m01,matrix.m11).magnitude;
        rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchoredPosition=new Vector2(center.x,-center.y);rt.sizeDelta=rect.size;
        rt.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(matrix.m10,matrix.m00)*Mathf.Rad2Deg);
        rt.localScale=new Vector3(sx,sy,1);
    }
    public void Image(Texture texture,Rect rect,Color color,Matrix4x4 matrix){
        var slot=Next(false);slot.Image.texture=texture;slot.Image.color=color;Place(slot.Image.rectTransform,rect,matrix);
    }
    public void Label(string value,Rect rect,int size,Color color,TextAnchor alignment,Matrix4x4 matrix){
        var slot=Next(true);slot.Text.text=value;slot.Text.fontSize=size;slot.Text.color=color;slot.Text.alignment=alignment;Place(slot.Text.rectTransform,rect,matrix);
    }
    public float TextWidth(string value,int size){
        return measure.GetPreferredWidth(value,new TextGenerationSettings{font=font,fontSize=size,fontStyle=FontStyle.Bold,scaleFactor=1,richText=false,lineSpacing=1,horizontalOverflow=HorizontalWrapMode.Overflow,verticalOverflow=VerticalWrapMode.Overflow,generationExtents=new Vector2(10000,1000),textAnchor=TextAnchor.MiddleLeft});
    }
    void OnDestroy(){((System.IDisposable)measure).Dispose();}
}
