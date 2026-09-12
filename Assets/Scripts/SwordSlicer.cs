using System.Collections.Generic;
using UnityEngine;

public class SwordSlicer : MonoBehaviour
{
    public Transform swordBase;
    public Transform swordTip;

    [Header("Movement Settings")]
    public float minimumMovement = 0.005f;

    [Header("Stroke End")]
    public float stopTime = 0.12f;

    [Header("Direction Settings")]
    [Range(0f, 1f)]
    public float directionThreshold = 0.5f;

    [Header("Hit Detection")]
    public float bladeRadius = 0.18f;

    private Vector3 previousBasePosition;
    private Vector3 previousTipPosition;

    private Vector3 previousMovementDirection;

    private float timeSinceMovement = 0f;

    // Fruits hit during the current stroke
    private HashSet<Fruit> fruitsHitThisStroke =
        new HashSet<Fruit>();

    void Start()
    {
        if (swordBase == null || swordTip == null)
        {
            Debug.LogError(
                "❌ SwordSlicer: Sword Base or Sword Tip is not assigned!"
            );

            enabled = false;
            return;
        }

        previousBasePosition = swordBase.position;
        previousTipPosition = swordTip.position;

        previousMovementDirection = Vector3.zero;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        Vector3 currentBasePosition = swordBase.position;
        Vector3 currentTipPosition = swordTip.position;

        Vector3 baseMovement =
            currentBasePosition - previousBasePosition;

        Vector3 tipMovement =
            currentTipPosition - previousTipPosition;

        float tipMovementDistance =
            tipMovement.magnitude;

        // =========================================
        // SWORD IS MOVING
        // =========================================

        if (tipMovementDistance > minimumMovement)
        {
            // Reset stop timer
            timeSinceMovement = 0f;

            Vector3 currentDirection =
                tipMovement.normalized;

            // =========================================
            // CHECK DIRECTION CHANGE
            // =========================================

            if (previousMovementDirection != Vector3.zero)
            {
                float directionDot =
                    Vector3.Dot(
                        previousMovementDirection,
                        currentDirection
                    );

                if (directionDot < directionThreshold)
                {
                    Debug.Log(
                        "🔄 DIRECTION CHANGED → NEW STROKE"
                    );

                    FinishStroke();
                }
            }

            previousMovementDirection =
                currentDirection;

            // =========================================
            // CHECK SWORD SWEEP
            // =========================================

            CheckBladeSweep(
                previousBasePosition,
                previousTipPosition,
                currentBasePosition,
                currentTipPosition
            );
        }
        else
        {
            // =========================================
            // SWORD HAS STOPPED
            // =========================================

            timeSinceMovement += Time.deltaTime;

            if (timeSinceMovement >= stopTime)
            {
                if (fruitsHitThisStroke.Count > 0)
                {
                    Debug.Log(
                        "⏸️ SWORD STOPPED → STROKE FINISHED"
                    );

                    FinishStroke();
                }

                // IMPORTANT:
                // Forget the previous direction.
                // The next movement starts a NEW stroke.
                previousMovementDirection =
                    Vector3.zero;

                timeSinceMovement = 0f;
            }
        }

        previousBasePosition =
            currentBasePosition;

        previousTipPosition =
            currentTipPosition;
    }

    // =========================================
    // BLADE SWEEP
    // =========================================

    void CheckBladeSweep(
        Vector3 oldBase,
        Vector3 oldTip,
        Vector3 newBase,
        Vector3 newTip
    )
    {
        CheckCapsuleCast(
            oldBase,
            oldTip,
            newBase - oldBase
        );

        CheckCapsuleCast(
            oldBase,
            oldTip,
            newTip - oldTip
        );

        CheckCurrentBlade(
            newBase,
            newTip
        );
    }

    // =========================================
    // CAPSULE CAST
    // =========================================

    void CheckCapsuleCast(
        Vector3 point1,
        Vector3 point2,
        Vector3 movement
    )
    {
        float distance =
            movement.magnitude;

        if (distance <= 0.0001f)
            return;

        Vector3 direction =
            movement.normalized;

        RaycastHit[] hits =
            Physics.CapsuleCastAll(
                point1,
                point2,
                bladeRadius,
                direction,
                distance
            );

        ProcessHits(hits);
    }

    // =========================================
    // CURRENT BLADE
    // =========================================

    void CheckCurrentBlade(
        Vector3 basePosition,
        Vector3 tipPosition
    )
    {
        Collider[] colliders =
            Physics.OverlapCapsule(
                basePosition,
                tipPosition,
                bladeRadius
            );

        foreach (Collider collider in colliders)
        {
            ProcessCollider(collider);
        }
    }

    // =========================================
    // PROCESS HITS
    // =========================================

    void ProcessHits(RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits)
        {
            ProcessCollider(hit.collider);
        }
    }

    void ProcessCollider(Collider collider)
    {
        if (collider == null || Time.timeScale == 0f)
            return;

        // =========================================
        // BOMB
        // =========================================

        Bomb bomb =
            collider.GetComponentInParent<Bomb>();

        if (bomb != null)
        {
            bomb.Explode();
            return;
        }

        // =========================================
        // FRUIT
        // =========================================

        Fruit fruit =
            collider.GetComponentInParent<Fruit>();

        if (fruit != null)
        {
            if (!fruitsHitThisStroke.Contains(fruit))
            {
                fruitsHitThisStroke.Add(fruit);

                Debug.Log(
                    "🍎 FRUIT HIT! " +
                    "Stroke fruits = " +
                    fruitsHitThisStroke.Count
                );

                fruit.Slice();
            }
        }
    }

    // =========================================
    // FINISH STROKE
    // =========================================

    void FinishStroke()
    {
        if (fruitsHitThisStroke.Count == 0)
            return;

        int fruitsInStroke =
            fruitsHitThisStroke.Count;

        Debug.Log(
            "🗡️ STROKE FINISHED → " +
            fruitsInStroke +
            " fruit(s)"
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessStroke(
                fruitsInStroke
            );
        }

        fruitsHitThisStroke.Clear();
    }

    // =========================================
    // DEBUG
    // =========================================

    void OnDrawGizmos()
    {
        if (swordBase == null ||
            swordTip == null)
            return;

        Gizmos.DrawLine(
            swordBase.position,
            swordTip.position
        );

        Gizmos.DrawWireSphere(
            swordBase.position,
            bladeRadius
        );

        Gizmos.DrawWireSphere(
            swordTip.position,
            bladeRadius
        );
    }
}