using UnityEngine;
using UnityEngine.InputSystem;

public class SwordController : MonoBehaviour
{
    public float moveSpeed = 15f;

    void Update()
    {
        if (Mouse.current == null || Camera.main == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // Keep the sword on the Z = 0 gameplay plane
        Vector3 screenPosition = new Vector3(
            mousePosition.x,
            mousePosition.y,
            -Camera.main.transform.position.z
        );

        Vector3 worldPosition =
            Camera.main.ScreenToWorldPoint(
                screenPosition
            );

        // Force Z to exactly 0
        worldPosition.z = 0f;

        transform.position = Vector3.Lerp(
            transform.position,
            worldPosition,
            moveSpeed * Time.deltaTime
        );
    }
}