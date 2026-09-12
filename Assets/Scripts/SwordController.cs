using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class SwordController : MonoBehaviour
{
    public enum ControlMode
    {
        Phone,
        Mouse
    }

    [Header("Input")]
    public ControlMode mode = ControlMode.Phone;
    public PhoneMotionReceiver phoneReceiver;

    [Header("Smoothing")]
    public float moveSpeed = 12f;

    [Header("Phone Tilt")]
    [Min(1f)]
    public float horizontalTiltDegrees = 45f;

    [Min(1f)]
    public float verticalTiltDegrees = 45f;

    public float tiltDeadZoneDegrees = 1f;

    [Header("Direction")]
    public bool invertHorizontal;
    public bool invertVertical;

    [Header("Acceleration Contribution")]
    public float gestureSensitivity = 0.35f;
    public float accelerationDeadZone = 0.3f;
    public float velocityDamping = 6f;
    public float maximumSpeed = 0.8f;

    [Tooltip("How quickly the acceleration offset returns to zero.")]
    public float offsetReturnSpeed = 2f;

    [Header("Screen Limits")]
    [Range(0f, 0.2f)]
    public float screenMargin = 0.05f;

    private Camera gameCamera;
    private Vector2 movementOffset;
    private Vector2 gestureVelocity;

    private double lastTimestamp = -1;
    private string centreID;
    private ControlMode previousMode;

    void Awake()
    {
        gameCamera = Camera.main;
        previousMode = mode;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            gestureVelocity = Vector2.zero;
            lastTimestamp = -1;
            return;
        }

        if (gameCamera == null)
            gameCamera = Camera.main;

        if (gameCamera == null)
            return;

        if (mode != previousMode)
        {
            ResetGesture();
            previousMode = mode;
        }

        Vector2 target;

        if (mode == ControlMode.Mouse)
        {
            if (Mouse.current == null)
                return;

            Vector2 mouse = Mouse.current.position.ReadValue();

            Vector3 viewport = gameCamera.ScreenToViewportPoint(
                new Vector3(mouse.x, mouse.y, 0f)
            );

            target = new Vector2(viewport.x, viewport.y);
        }
        else
        {
            if (phoneReceiver == null || !phoneReceiver.receiving)
            {
                gestureVelocity = Vector2.zero;
                lastTimestamp = -1;
                return;
            }

            if (centreID != phoneReceiver.CentreID)
            {
                centreID = phoneReceiver.CentreID;
                ResetGesture();
            }

            UpdateAcceleration();

            // Fade the extra movement back toward the tilt position.
            movementOffset *= Mathf.Exp(
                -Mathf.Max(0f, offsetReturnSpeed) * Time.deltaTime
            );

            // With a flat, screen-up phone:
            // Camera end raised = positive pitch = sword upward.
            // Charging-port end raised = negative pitch = sword downward.
            float horizontal = NormalizeTilt(
                phoneReceiver.roll,
                horizontalTiltDegrees
            );

            float vertical = NormalizeTilt(
                phoneReceiver.pitch,
                verticalTiltDegrees
            );

            if (invertHorizontal)
                horizontal = -horizontal;

            if (invertVertical)
                vertical = -vertical;

            float travel = 0.5f - screenMargin;

            Vector2 tiltPosition = new Vector2(
                0.5f + horizontal * travel,
                0.5f + vertical * travel
            );

            target = tiltPosition + movementOffset;
            Vector2 limitedTarget = ClampToScreen(target);

            // Remove excess offset when reaching a screen edge.
            movementOffset += limitedTarget - target;

            if (target.x != limitedTarget.x)
                gestureVelocity.x = 0f;

            if (target.y != limitedTarget.y)
                gestureVelocity.y = 0f;

            target = limitedTarget;
        }

        MoveSword(ClampToScreen(target));
    }

    private float NormalizeTilt(float angle, float fullRange)
    {
        float range = Mathf.Max(1f, fullRange);
        float deadZone = Mathf.Clamp(
            tiltDeadZoneDegrees,
            0f,
            range - 0.01f
        );

        float magnitude = Mathf.Max(
            0f,
            Mathf.Abs(angle) - deadZone
        );

        return Mathf.Sign(angle) * Mathf.Clamp01(
            magnitude / (range - deadZone)
        );
    }

    private void UpdateAcceleration()
    {
        if (gestureSensitivity <= 0f)
        {
            movementOffset = Vector2.zero;
            gestureVelocity = Vector2.zero;
            lastTimestamp = phoneReceiver.Timestamp;
            return;
        }

        double timestamp = phoneReceiver.Timestamp;

        if (timestamp == lastTimestamp)
            return;

        if (lastTimestamp < 0)
        {
            lastTimestamp = timestamp;
            return;
        }

        float dt = (float)(timestamp - lastTimestamp);
        lastTimestamp = timestamp;

        if (dt <= 0f || dt > 0.1f)
        {
            gestureVelocity = Vector2.zero;
            return;
        }

        Vector2 acceleration = new Vector2(
            ApplyAccelerationDeadZone(phoneReceiver.ax),
            ApplyAccelerationDeadZone(phoneReceiver.ay)
        );

        if (invertHorizontal)
            acceleration.x = -acceleration.x;

        if (invertVertical)
            acceleration.y = -acceleration.y;

        acceleration = Vector2.ClampMagnitude(acceleration, 15f);

        gestureVelocity += acceleration * gestureSensitivity * dt;

        float damping = Mathf.Max(0f, velocityDamping);

        if (acceleration.sqrMagnitude < 0.001f)
            damping *= 3f;

        gestureVelocity *= Mathf.Exp(-damping * dt);

        gestureVelocity = Vector2.ClampMagnitude(
            gestureVelocity,
            Mathf.Max(0f, maximumSpeed)
        );

        movementOffset += gestureVelocity * dt;
    }

    private float ApplyAccelerationDeadZone(float value)
    {
        return Mathf.Sign(value) * Mathf.Max(
            0f,
            Mathf.Abs(value) - Mathf.Max(0f, accelerationDeadZone)
        );
    }

    private Vector2 ClampToScreen(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, screenMargin, 1f - screenMargin),
            Mathf.Clamp(position.y, screenMargin, 1f - screenMargin)
        );
    }

    private void MoveSword(Vector2 viewportPosition)
    {
        Ray ray = gameCamera.ViewportPointToRay(
            new Vector3(viewportPosition.x, viewportPosition.y, 0f)
        );

        Plane gameplayPlane = new Plane(
            Vector3.forward,
            Vector3.zero
        );

        if (!gameplayPlane.Raycast(ray, out float distance))
            return;

        float blend = 1f - Mathf.Exp(
            -Mathf.Max(0f, moveSpeed) * Time.deltaTime
        );

        Vector3 position = Vector3.Lerp(
            transform.position,
            ray.GetPoint(distance),
            blend
        );

        position.z = 0f;
        transform.position = position;
    }

    private void ResetGesture()
    {
        movementOffset = Vector2.zero;
        gestureVelocity = Vector2.zero;
        lastTimestamp = -1;
    }
}