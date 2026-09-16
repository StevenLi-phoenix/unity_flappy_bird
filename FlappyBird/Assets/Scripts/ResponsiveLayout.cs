using UnityEngine;
// One conversion between physical pixels and Canvas reference units. World and UI
// share the same scale; only their anchors differ (ground vs. screen center).
public readonly struct ResponsiveLayout {
    public readonly float Scale,Width,Height;
    public Vector2 UiOrigin=>new Vector2((Width-GameLayout.Width)/2,(Height-GameLayout.Height)/2);
    public Vector2 WorldOrigin=>new Vector2(UiOrigin.x,Height-GameLayout.Height);
    public Rect WorldBounds=>new Rect(-WorldOrigin.x,-WorldOrigin.y,Width,Height);
    public Rect SoundButton=>new Rect(Width-UiOrigin.x-100,24-UiOrigin.y,64,64);
    public ResponsiveLayout(float pixelWidth,float pixelHeight){
        Scale=Mathf.Max(0.001f,Mathf.Min(pixelWidth/GameLayout.Width,pixelHeight/GameLayout.Height));
        Width=pixelWidth/Scale;Height=pixelHeight/Scale;
    }
    public Vector2 PixelToUi(Vector2 topLeftPixels)=>topLeftPixels/Scale-UiOrigin;
    public Vector2 UiToPixel(Vector2 point)=>(point+UiOrigin)*Scale;
    public Vector2 WorldToPixel(Vector2 point)=>(point+WorldOrigin)*Scale;
}
