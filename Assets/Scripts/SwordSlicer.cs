
using System.Collections.Generic;
using UnityEngine;

public class SwordSlicer : MonoBehaviour
{
    public Transform swordBase;
    public Transform swordTip;

    [Header("Stroke Settings")]
    public float minimumMovement = 0.02f;
    public float strokeEndTime = 0.12f;

    private Vector3 previousBasePosition;
    private Vector3 previousTipPosition;

    private float timeSinceMovement = 0f;

    // Fruits already hit during THIS stroke
    private HashSet<Fruit> fruitsHitThisStroke = new HashSet<Fruit>();

    void Start()
    {
        previousBasePosition = swordBase.position;
        previousTipPosition = swordTip.position;
    }

    void Update()
    {
        Vector3 currentBasePosition = swordBase.position;
        Vector3 currentTipPosition = swordTip.position;

        float movement =
            Vector3.Distance(previousBasePosition, currentBasePosition) +
            Vector3.Distance(previousTipPosition, currentTipPosition);

        if (movement > minimumMovement)
        {
            timeSinceMovement = 0f;

            // Check the blade movement
            CheckBlade(previousBasePosition, currentBasePosition);
            CheckBlade(previousTipPosition, currentTipPosition);

            // Diagonal checks
            CheckBlade(previousBasePosition, currentTipPosition);
            CheckBlade(previousTipPosition, currentBasePosition);
        }
        else
        {
            timeSinceMovement += Time.deltaTime;

            // Sword stopped moving → finish the stroke
            if (timeSinceMovement >= strokeEndTime &&
                fruitsHitThisStroke.Count > 0)
            {
                FinishStroke();
            }
        }

        previousBasePosition = currentBasePosition;
        previousTipPosition = currentTipPosition;
    }

    void CheckBlade(Vector3 start, Vector3 end)
    {
        if (Physics.Linecast(start, end, out RaycastHit hit))
        {
            // 💣 Bomb
            Bomb bomb = hit.collider.GetComponent<Bomb>();

            if (bomb != null)
            {
                Debug.Log("💣 BOMB HIT!");

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }

                return;
            }

            // 🍎 Fruit
            Fruit fruit = hit.collider.GetComponent<Fruit>();

            if (fruit != null)
            {
                // Prevent the same fruit being counted multiple times
                // during the same sword stroke.
                if (!fruitsHitThisStroke.Contains(fruit))
                {
                    fruitsHitThisStroke.Add(fruit);

                    Debug.Log(
                        "🍎 Fruit hit in current stroke: " +
                        fruitsHitThisStroke.Count
                    );

                    fruit.Slice();
                }
            }
        }
    }

    void FinishStroke()
    {
        int fruitsHit = fruitsHitThisStroke.Count;

        Debug.Log(
            "🗡️ STROKE FINISHED — Fruits sliced: " +
            fruitsHit
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessStroke(fruitsHit);
        }

        fruitsHitThisStroke.Clear();
    }
}
