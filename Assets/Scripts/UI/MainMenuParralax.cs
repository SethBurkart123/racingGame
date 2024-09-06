using UnityEngine;

public class MainMenuParralax : MonoBehaviour
{
    public Camera mainCamera;
    public Canvas targetCanvas;
    public float parallaxFactor = 0.1f;
    public float smoothness = 5f;

    private Vector3 initialCameraPosition;
    private Vector3 initialCanvasPosition;
    private Vector2 mousePosition;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (targetCanvas == null)
            targetCanvas = GetComponent<Canvas>();

        initialCameraPosition = mainCamera.transform.position;
        initialCanvasPosition = targetCanvas.transform.position;
    }

    void Update()
    {
        // Get mouse position in screen space (0 to 1)
        mousePosition.x = Input.mousePosition.x / Screen.width;
        mousePosition.y = Input.mousePosition.y / Screen.height;

        // Calculate offset based on mouse position
        Vector2 offset = (mousePosition - new Vector2(0.5f, 0.5f)) * parallaxFactor;

        // Apply parallax to camera
        Vector3 targetCameraPosition = initialCameraPosition + new Vector3(offset.x, offset.y, 0);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCameraPosition, Time.deltaTime * smoothness);

        // Apply parallax to canvas (in opposite direction for depth effect)
        Vector3 targetCanvasPosition = initialCanvasPosition - new Vector3(offset.x, offset.y, 0);
        targetCanvas.transform.position = Vector3.Lerp(targetCanvas.transform.position, targetCanvasPosition, Time.deltaTime * smoothness);
    }
}