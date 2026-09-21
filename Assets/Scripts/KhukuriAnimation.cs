using UnityEngine;

public class KhukuriAnimation : MonoBehaviour
{
    [Header("References")]
    public Transform khukuriModel;
    public Camera gameCamera;

    [Header("Movement Tilt")]
    public float sidewaysTurn = 35f;
    public float forwardTilt = 25f;
    public float sidewaysLean = 18f;

    [Tooltip("Movement speed in viewport units needed for full tilt.")]
    public float speedForFullTilt = 0.7f;

    public float tiltSmoothing = 10f;

    [Header("Cut Animation")]
    public float cutDuration = 0.25f;
    public float cutTurn = 30f;
    public float cutLean = 20f;

    private Quaternion restingLocalRotation;
    private Vector3 previousPosition;
    private Vector2 smoothMovement;
    private Vector2 lastDirection = Vector2.right;
    private Vector2 cutDirection;

    private float cutTimer;
    private bool cutting;
    private bool initialized;

    void Start()
    {
        if (khukuriModel == null || khukuriModel == transform)
        {
            Debug.LogError(
                "KhukuriAnimation: Assign the khukuri model child."
            );

            enabled = false;
            return;
        }

        if (gameCamera == null)
            gameCamera = Camera.main;

        // Remember your sharp-edge-facing-camera orientation.
        restingLocalRotation = khukuriModel.localRotation;
        previousPosition = transform.position;
        initialized = true;
    }

    void LateUpdate()
    {
        if (!initialized)
            return;

        if (Time.timeScale == 0f)
        {
            previousPosition = transform.position;
            return;
        }

        if (gameCamera == null)
            gameCamera = Camera.main;

        if (gameCamera == null)
            return;

        float dt = Time.deltaTime;

        if (dt <= 0f)
            return;

        Vector3 previousViewport =
            gameCamera.WorldToViewportPoint(previousPosition);

        Vector3 currentViewport =
            gameCamera.WorldToViewportPoint(transform.position);

        Vector2 velocity = new Vector2(
            currentViewport.x - previousViewport.x,
            currentViewport.y - previousViewport.y
        ) / dt;

        previousPosition = transform.position;

        Vector2 movement = Vector2.ClampMagnitude(
            velocity / Mathf.Max(0.01f, speedForFullTilt),
            1f
        );

        if (movement.sqrMagnitude > 0.01f)
            lastDirection = movement.normalized;

        float blend = 1f - Mathf.Exp(
            -Mathf.Max(0f, tiltSmoothing) * dt
        );

        smoothMovement = Vector2.Lerp(
            smoothMovement,
            movement,
            blend
        );

        // Camera-relative tilt: pitch, turn, and sideways lean.
        Vector3 angles = new Vector3(
            smoothMovement.y * forwardTilt,
            smoothMovement.x * sidewaysTurn,
            -smoothMovement.x * sidewaysLean
        );

        if (cutting)
        {
            cutTimer += dt;

            float progress = Mathf.Clamp01(
                cutTimer / Mathf.Max(0.01f, cutDuration)
            );

            // Smooth outward swing followed by recovery.
            float swing = Mathf.Sin(progress * Mathf.PI);

            angles.x += cutDirection.y * cutTurn * swing;
            angles.y += cutDirection.x * cutTurn * swing;
            angles.z -= cutDirection.x * cutLean * swing;

            if (progress >= 1f)
                cutting = false;
        }

        Quaternion restingWorldRotation =
            khukuriModel.parent != null
                ? khukuriModel.parent.rotation * restingLocalRotation
                : restingLocalRotation;

        Quaternion cameraRotation = gameCamera.transform.rotation;

        Quaternion tilt =
            cameraRotation *
            Quaternion.Euler(angles) *
            Quaternion.Inverse(cameraRotation);

        khukuriModel.rotation = tilt * restingWorldRotation;
    }

    public void PlayCutAnimation()
    {
        // Several fruits in one swing should not restart the animation.
        if (!initialized || cutting || Time.timeScale == 0f)
            return;

        cutDirection = lastDirection;
        cutTimer = 0f;
        cutting = true;
    }

    void OnDisable()
    {
        if (initialized && khukuriModel != null)
            khukuriModel.localRotation = restingLocalRotation;

        cutting = false;
        smoothMovement = Vector2.zero;
    }

    void OnEnable()
    {
        previousPosition = transform.position;
    }
}