using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectTweaks : MonoBehaviour
{
    [Tooltip("Mouse wheel sensitivity (default Unity is ~10)")]
    public float scrollSensitivity = 60f;

    [Tooltip("How fast inertia slows down. 0 = instant stop, 1 = never stops.")]
    public float decelerationRate = 0.135f;

    public ScrollRect.MovementType movement = ScrollRect.MovementType.Clamped;

    void Awake()
    {
        var sr = GetComponent<ScrollRect>();
        sr.scrollSensitivity = scrollSensitivity;
        sr.decelerationRate  = decelerationRate;
        sr.movementType      = movement;
    }
}

