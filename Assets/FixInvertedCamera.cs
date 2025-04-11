using UnityEngine;

public class FixInvertedCamera : MonoBehaviour
{
    public bool invertHorizontal = false; // Toggle if left/right is reversed
    public bool invertVertical = true;   // Usually, vertical look is inverted by default

    private Vector3 lastMousePosition;
    private float rotationSpeed = 2f;

    void Update()
    {
        if (Input.GetMouseButton(1)) // Right-click to look around
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // Apply inversion if needed
            if (invertHorizontal) mouseX *= -1;
            if (invertVertical) mouseY *= -1;

            // Rotate the camera
            transform.Rotate(Vector3.up, mouseX, Space.World);       // Horizontal (Y-axis)
            transform.Rotate(Vector3.right, -mouseY, Space.Self);   // Vertical (X-axis)
        }
    }
}