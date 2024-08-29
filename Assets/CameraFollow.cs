using UnityEngine;

public class SmoothCarCamera : MonoBehaviour
{
    public Transform target;  // The car to follow
    public float distanceBehind = 6.0f;  // Distance behind the car
    public float heightAbove = 2.0f;  // Height above the car
    public float followSpeed = 5.0f;  // Speed of camera movement
    public float rotationSpeed = 3.0f;  // Speed of camera rotation
    public float lookAheadDistance = 5.0f;  // Distance to look ahead of the car
    public float lateralOffset = 2.0f;  // Maximum lateral offset during turns
    public float maxSpeedForHeightChange = 100f;  // Speed at which max height is reached
    public float maxAdditionalHeight = 3f;  // Maximum additional height at high speeds
    public float heightChangeMultiplier = 1f;  // Multiplier for height change effect
    public float minFOV = 60f;  // Minimum FOV at low speeds
    public float maxFOV = 75f;  // Maximum FOV at high speeds
    public float fovChangeSpeed = 2f;  // Speed of FOV change

    private Vector3 desiredPosition;
    private Quaternion desiredRotation;
    private Vector3 smoothVelocity;
    private float currentLateralOffset;
    private float currentAdditionalHeight;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found on this GameObject!");
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("No target set for the camera to follow!");
            return;
        }

        Vector3 targetForward = target.forward;
        Vector3 targetRight = target.right;
        Vector3 targetUp = target.up;

        // Calculate lateral offset based on car's rotation
        float targetRotationY = target.eulerAngles.y;
        float cameraRotationY = transform.eulerAngles.y;
        float rotationDifference = Mathf.DeltaAngle(cameraRotationY, targetRotationY);
        float targetLateralOffset = Mathf.Clamp(rotationDifference / 45f, -1f, 1f) * lateralOffset;

        // Smoothly interpolate current lateral offset
        currentLateralOffset = Mathf.Lerp(currentLateralOffset, targetLateralOffset, Time.deltaTime * followSpeed);

        // Get the car's current speed (assuming the target has a Rigidbody component)
        float carSpeed = target.GetComponent<Rigidbody>().velocity.magnitude;

        // Calculate additional height based on speed
        float speedRatio = Mathf.Clamp01(carSpeed / maxSpeedForHeightChange);
        float targetAdditionalHeight = speedRatio * maxAdditionalHeight * heightChangeMultiplier;
        currentAdditionalHeight = Mathf.Lerp(currentAdditionalHeight, targetAdditionalHeight, Time.deltaTime * followSpeed);

        // Calculate the desired position with lateral offset and additional height
        Vector3 lookAheadPos = target.position + targetForward * lookAheadDistance;
        desiredPosition = target.position 
            - targetForward * distanceBehind 
            + targetUp * (heightAbove + currentAdditionalHeight)
            + targetRight * currentLateralOffset;

        // Smoothly move the camera towards the desired position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, 1f / followSpeed);

        // Calculate and set the desired rotation
        desiredRotation = Quaternion.LookRotation(lookAheadPos - transform.position, targetUp);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);

        // Adjust FOV based on speed
        float targetFOV = Mathf.Lerp(minFOV, maxFOV, speedRatio);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }
}