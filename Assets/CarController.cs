using UnityEngine;

public class HighPerformanceRacingCarController : MonoBehaviour
{
    [Header("Car Specifications")]
    public float enginePower = 1000000f; // Drastically increased
    public float maxMotorTorque = 10000f; // Significantly increased
    public float maxSteeringAngle = 40f; // Increased for sharper turns
    public float maxSpeed = 400f; // Increased max speed (km/h)
    public float accelerationFactor = 50f; // Drastically increased
    public float brakeTorque = 50000f; // Increased for stronger brakes
    public float driftFactor = 0.95f; // Add this line back

    // Wheel Colliders
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    // Wheel Transforms
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    private Rigidbody rb;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(0, 3, -7);

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1000f; // Reduced mass for faster acceleration
        rb.drag = 0.01f; // Reduced drag for less air resistance
        rb.angularDrag = 0.05f;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        UpdateCamera();
        ConfigureWheelColliders();
    }

    private void ConfigureWheelColliders()
    {
        WheelCollider[] wheels = { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };
        foreach (var wheel in wheels)
        {
            wheel.suspensionDistance = 0.1f; // Reduced for stiffer suspension
            wheel.forceAppPointDistance = 0;
            
            WheelFrictionCurve fwdFriction = wheel.forwardFriction;
            fwdFriction.stiffness = 4f; // Increased for better traction
            wheel.forwardFriction = fwdFriction;

            WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
            sideFriction.stiffness = 4f; // Increased for better cornering
            wheel.sidewaysFriction = sideFriction;
        }
    }

    private void FixedUpdate()
    {
        float accelerationInput = Input.GetAxis("Vertical");
        float steeringInput = Input.GetAxis("Horizontal");

        HandleDriving(accelerationInput, steeringInput);
        UpdateWheels();
        UpdateCamera();
    }

    private void HandleDriving(float accelerationInput, float steeringInput)
    {
        float currentSpeed = rb.velocity.magnitude * 3.6f; // Convert to km/h
        
        // Calculate motor torque, allowing for reverse
        float motorTorque = enginePower * maxMotorTorque * Mathf.Abs(accelerationInput) * 
                            Mathf.Lerp(1f, 0.1f, currentSpeed / maxSpeed) * accelerationFactor;
        
        // Determine direction (forward or reverse)
        float direction = accelerationInput >= 0 ? 1 : -1;
        
        ApplyTorqueToWheels(motorTorque * direction);

        float steeringAngle = steeringInput * maxSteeringAngle;
        ApplySteering(steeringAngle);

        // Apply drifting
        ApplyDrift(steeringInput, currentSpeed);

        // Apply brakes when not accelerating
        float brake = Mathf.Abs(accelerationInput) < 0.1f ? brakeTorque : 0f;
        ApplyBrakes(brake);
    }

    private void ApplyTorqueToWheels(float motorTorque)
    {
        // Apply more torque to rear wheels for a rear-wheel drive feel
        rearLeftWheel.motorTorque = motorTorque * 0.7f;
        rearRightWheel.motorTorque = motorTorque * 0.7f;
        frontLeftWheel.motorTorque = motorTorque * 0.3f;
        frontRightWheel.motorTorque = motorTorque * 0.3f;
    }

    private void ApplySteering(float steeringAngle)
    {
        frontLeftWheel.steerAngle = steeringAngle;
        frontRightWheel.steerAngle = steeringAngle;
    }

    private void ApplyDrift(float steeringInput, float currentSpeed)
    {
        if (Mathf.Abs(steeringInput) > 0.1f && currentSpeed > 10f)
        {
            Vector3 driftForce = -transform.right * steeringInput * driftFactor;
            rb.AddForce(driftForce, ForceMode.Force);
        }
    }

    private void ApplyBrakes(float brakeTorque)
    {
        WheelCollider[] wheels = { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakeTorque;
        }
    }

    private void UpdateWheels()
    {
        UpdateWheelPos(frontLeftWheel, frontLeftTransform);
        UpdateWheelPos(frontRightWheel, frontRightTransform);
        UpdateWheelPos(rearLeftWheel, rearLeftTransform);
        UpdateWheelPos(rearRightWheel, rearRightTransform);
    }

    private void UpdateWheelPos(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    private void UpdateCamera()
    {
        mainCamera.transform.position = transform.TransformPoint(cameraOffset);
        mainCamera.transform.LookAt(transform.position);
    }
}