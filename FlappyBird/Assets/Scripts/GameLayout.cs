using UnityEngine;
public static class GameLayout {
    public const float Width = 1100, Height = 780, PlayfieldTop = 90;
    public const float FirstPipeX = 5.7f;
    public static Rect Viewport(float width, float height) {
        float scale = Mathf.Min(width / Width, height / Height);
        return new Rect((width - Width * scale) / 2, (height - Height * scale) / 2, Width * scale, Height * scale);
    }
    public static float AnimationStep(bool dead, float deltaTime) => dead ? 0 : deltaTime;
}
