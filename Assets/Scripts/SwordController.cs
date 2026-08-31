using UnityEngine;
using UnityEngine.InputSystem;

public class SwordController : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float swordDistance = 5f;

    void Update()
    {
        if (Mouse.current == null || Camera.main == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector3 screenPosition = new Vector3(
            mousePosition.x,
            mousePosition.y,
            swordDistance
        );

        Vector3 worldPosition =
            Camera.main.ScreenToWorldPoint(screenPosition);

        transform.position = Vector3.Lerp(
            transform.position,
            worldPosition,
            moveSpeed * Time.deltaTime
        );
    }
}