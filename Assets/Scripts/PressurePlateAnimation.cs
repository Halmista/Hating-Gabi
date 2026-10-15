
using UnityEngine;

public class PressurePlateAnimation : MonoBehaviour
{
    [Header("Visual")]
    public Transform plateVisual;

    [Header("Animation Settings")]
    public float sinkDistance = 0.12f;
    public float animationSpeed = 10f;

    private Vector3 raisedPosition;
    private Vector3 pressedPosition;
    private bool isPressed;

    private void Awake()
    {
        if (plateVisual == null)
        {
            Debug.LogError(
                "Assign Plate Visual on " + gameObject.name
            );
            enabled = false;
            return;
        }

        raisedPosition = plateVisual.localPosition;
        pressedPosition = raisedPosition +
            Vector3.down * sinkDistance;
    }

    private void Update()
    {
        Vector3 targetPosition = isPressed
            ? pressedPosition
            : raisedPosition;

        plateVisual.localPosition = Vector3.Lerp(
            plateVisual.localPosition,
            targetPosition,
            1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
        );
    }

    public void SetPressed(bool pressed)
    {
        isPressed = pressed;
    }
}
