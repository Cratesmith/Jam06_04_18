using UnityEngine;

public static class FloatExtensions
{
    public static Vector2 AngleToVector2(this float @this)
    {
        @this *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(@this), Mathf.Sin(@this));
    }
}