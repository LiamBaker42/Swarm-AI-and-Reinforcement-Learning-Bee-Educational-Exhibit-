using UnityEngine;
using UnityEngine.EventSystems;

public class RTS_Camera : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float panSpeed = 20f;
    public float zoomSpeed = 25f;
    public float rotationSpeed = 50f;
    public float touchPanSensitivity = 0.05f; 

    [Header("Follow Mode")]
    public Vector3 followOffset = new Vector3(0, 15f, -18f);
    private Transform followTarget = null;

    [Header("Zoom Limits")]
    public float minY = 5f;
    public float maxY = 80f;

    void LateUpdate()
    {
        float scrollInput = GetZoomInput();
        Vector3 touchPanInput = GetTouchPanInput();

        // ROTATION
        if (Input.GetKey("q")) transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey("e")) transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // FOLLOW MODE
        if (followTarget != null)
        {
            Vector3 rotationRelativeOffset = transform.rotation * new Vector3(0, 0, -followOffset.magnitude);
            transform.position = followTarget.position + rotationRelativeOffset;

            Vector3 currentPos = transform.position;
            currentPos.y = followTarget.position.y + followOffset.y;
            transform.position = currentPos;

            // Follow Mode Zooming (Keyboard + Touch)
            if (scrollInput != 0)
            {
                followOffset.y -= scrollInput * zoomSpeed;
                followOffset.z += scrollInput * zoomSpeed;

                followOffset.y = Mathf.Clamp(followOffset.y, 3f, 60f);
                followOffset.z = Mathf.Clamp(followOffset.z, -50f, -4f);
            }
        }
        // RTS MODE
        else
        {
            Vector3 pos = transform.position;
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = transform.right;

            // Handle WASD Keyboard Input
            if (Input.GetKey("w")) pos += forward * panSpeed * Time.deltaTime;
            if (Input.GetKey("s")) pos -= forward * panSpeed * Time.deltaTime;
            if (Input.GetKey("d")) pos += right * panSpeed * Time.deltaTime;
            if (Input.GetKey("a")) pos -= right * panSpeed * Time.deltaTime;

            // Handle Touch Panning Input
            if (touchPanInput != Vector3.zero)
            {
                pos += (forward * touchPanInput.y + right * touchPanInput.x) * panSpeed * touchPanSensitivity;
            }

            // Handle Zooming (Mouse + Touch)
            pos.y -= scrollInput * zoomSpeed * 100f * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.Space)) ClearFollowTarget();
    }

    // Helper to get Zoom from Mouse or Pinch
    private float GetZoomInput()
    {
        // Check Mouse first
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0) return mouseScroll;

        // Check Pinch Gesture
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevMag = (t0Prev - t1Prev).magnitude;
            float currentMag = (t0.position - t1.position).magnitude;

            // Return a normalized difference
            return (currentMag - prevMag) * 0.01f;
        }

        return 0;
    }

    // Helper to get Pan from Touch
    private Vector3 GetTouchPanInput()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            // Ignore touch if it started on a UI button
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return Vector3.zero;

            if (t.phase == TouchPhase.Moved)
            {
                // Return the movement delta
                return new Vector3(t.deltaPosition.x, t.deltaPosition.y, 0);
            }
        }
        return Vector3.zero;
    }

    public void SetFollowTarget(Transform target) => followTarget = target;
    public void ClearFollowTarget() => followTarget = null;
}