using UnityEngine;
using UnityEngine.InputSystem;

public class HeadLookController
{
    private readonly InputAction lookAction;
    private float yaw;
    private float pitch;
    private readonly float yawSpeed;
    private readonly float pitchSpeed;
    private readonly float pitchMin;
    private readonly float pitchMax;

    public HeadLookController(
        InputAction lookAction,
        float yawSpeedDegPerSec = 90f,
        float pitchSpeedDegPerSec = 60f,
        float pitchMinDeg = -60f,
        float pitchMaxDeg = 60f)
    {
        this.lookAction = lookAction;
        yawSpeed = yawSpeedDegPerSec;
        pitchSpeed = pitchSpeedDegPerSec;
        pitchMin = pitchMinDeg;
        pitchMax = pitchMaxDeg;
    }

    public void EnterHeadLook()
    {
        lookAction?.Enable();

        var cam = Camera.main;
        if (cam == null) return;

        var e = cam.transform.rotation.eulerAngles;
        yaw = e.y;
        pitch = e.x;
        if (pitch > 180f) pitch -= 360f; // 0..360 -> -180..180
    }

    public void UpdateHeadLook()
    {
        var cam = Camera.main;
        if (cam == null || lookAction == null) return;

        Vector2 input = lookAction.ReadValue<Vector2>();
        if (input.sqrMagnitude < 0.0001f) return;

        // x: left/right, y: up/down
        yaw += input.x * yawSpeed * Time.deltaTime; // yaw = dévier, gauche <-> droite
        pitch -= input.y * pitchSpeed * Time.deltaTime; // pitch = basculer, haut <-> bas

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void ExitHeadLook()
    {
        lookAction?.Disable();
    }
}
