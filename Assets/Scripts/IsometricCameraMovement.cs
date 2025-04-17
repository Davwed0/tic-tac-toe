using UnityEngine;

public class IsometricCameraMovement : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float angularSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;

    private float targetAngle = 0f;
    private float currentAngle = 0f;

    private const float SNAP_ANGLE = 45f;
    private const float TILT_ANGLE = 26.565f;

    private void Start()
    {
        PlayerColor player = NetworkManager.Instance.IsMasterClient() ? PlayerColor.WHITE : PlayerColor.BLACK;
        if (player == PlayerColor.WHITE)
        {
            targetAngle = 225f;
        }
        else
        {
            targetAngle = 45f;
        }
    }

    private void Update()
    {
        HandleMouseInput();
        NormalizeTargetAngle();
        UpdateCameraRotation();
        HandleZoom();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetAngle += mouseX * mouseSensitivity;
        }
        else
        {
            targetAngle = Mathf.Round(targetAngle / SNAP_ANGLE) * SNAP_ANGLE;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float zoom = Camera.main.orthographicSize - scroll;

        Camera.main.orthographicSize = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    private void NormalizeTargetAngle()
    {
        targetAngle = (targetAngle % 360f + 360f) % 360f;
    }

    private void UpdateCameraRotation()
    {
        currentAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, angularSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(TILT_ANGLE, currentAngle, 0);
    }
}
