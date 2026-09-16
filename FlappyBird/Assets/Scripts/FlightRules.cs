using UnityEngine;
public static class FlightRules {
    public const float Gravity = -15f, Flap = 5.3f, Gap = 2.65f;
    public static bool Hit(float x, float y, float pipeX, float gapY) {
        return Mathf.Abs(x-pipeX)<0.81f && (y-0.24f < gapY-Gap/2 || y+0.24f > gapY+Gap/2);
    }
    public static bool Bounds(float y) => y < -3.25f || y > 4.65f;
}
