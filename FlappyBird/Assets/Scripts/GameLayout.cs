using UnityEngine;
public static class GameLayout {
    public const float Width = 1100, Height = 660, PlayfieldTop = 0;
    public const float PixelsPerUnit = 61, PipeHalfWidth = 41, PipeSpacing = 4;
    public const float FirstPipeX = (Width / 2 + PipeHalfWidth + 1) / PixelsPerUnit;
    public static float SpawnPipeX(float right=Width)=>(right-Width/2+PipeHalfWidth+1)/PixelsPerUnit;
    public static bool PipeHasExited(float x,float left=0) => Width / 2 + x * PixelsPerUnit + PipeHalfWidth < left;
    public static float NextPipeX(float lastX,float right=Width) => Mathf.Max(SpawnPipeX(right), lastX + PipeSpacing);
    public static float AnimationStep(bool dead, float deltaTime) => dead ? 0 : deltaTime;
}
