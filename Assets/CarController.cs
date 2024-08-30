using UnityEngine;

public class RacinCarController : MonoBehaviour
{
    public WheelCollider WheelFL, WheelFR, WheelRL, WheelRR;
    public Transform WheelFLtrans, WheelFRtrans, WheelRLtrans, WheelRRtrans;
    public Vector3 eulertest;
    public Transform centreofmass;

    private bool braked = false;
    private float maxBrakeTorque = 500;
    private Rigidbody rb;
    [Header("Engine Settings")]
    public float maxTorque = 2000f;
    public float accelerationFactor = 5f;
    [Range(0.1f, 1f)]
    public float accelerationCurve = 0.5f;

    [Header("Steering Settings")]
    public float maxSteeringAngle = 30f;
    public float minSteeringAngle = 10f;
    [Range(0f, 1f)]
    public float steeringReductionFactor = 0.7f;

    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centreofmass.transform.localPosition;
    }

    void FixedUpdate()
    {
        if (!braked)
        {
            WheelFL.brakeTorque = WheelFR.brakeTorque = WheelRL.brakeTorque = WheelRR.brakeTorque = 0;
        }

        // Speed of car with responsive acceleration
        float accelerationInput = Input.GetAxis("Vertical");
        float currentTorque = maxTorque * Mathf.Sign(accelerationInput) * Mathf.Pow(Mathf.Abs(accelerationInput), accelerationFactor);
        currentTorque *= Mathf.Lerp(1f, Mathf.Abs(accelerationInput), accelerationCurve);
        WheelRR.motorTorque = WheelRL.motorTorque = currentTorque;

        // Calculate current speed
        currentSpeed = rb.velocity.magnitude;

        // Calculate steering angle based on speed
        float speedFactor = Mathf.Clamp01(currentSpeed / 100f); // Adjust 100f to change the speed at which steering reduction maxes out
        float currentSteeringAngle = Mathf.Lerp(maxSteeringAngle, minSteeringAngle, speedFactor * steeringReductionFactor);

        // Changing car direction with speed-based steering angle
        WheelFL.steerAngle = WheelFR.steerAngle = currentSteeringAngle * Input.GetAxis("Horizontal");

        // Changing car direction
        WheelFL.steerAngle = WheelFR.steerAngle = 30 * Input.GetAxis("Horizontal");
    }

    void Update()
    {
        HandBrake();
        
        // Rotate wheels
        RotateWheel(WheelFLtrans, WheelFL.rpm);
        RotateWheel(WheelFRtrans, WheelFR.rpm);
        RotateWheel(WheelRLtrans, WheelRL.rpm);
        RotateWheel(WheelRRtrans, WheelRR.rpm);

        // Change front wheel direction
        UpdateWheelDirection(WheelFLtrans, WheelFL.steerAngle);
        UpdateWheelDirection(WheelFRtrans, WheelFR.steerAngle);

        eulertest = WheelFLtrans.localEulerAngles;
    }

    void HandBrake()
    {
        braked = Input.GetButton("Jump");

        if (braked)
        {
            WheelRL.brakeTorque = WheelRR.brakeTorque = maxBrakeTorque * 20;
            WheelRL.motorTorque = WheelRR.motorTorque = 0;
        }
    }

    void RotateWheel(Transform wheelTransform, float rpm)
    {
        wheelTransform.Rotate(rpm / 60 * 360 * Time.deltaTime, 0, 0);
    }

    void UpdateWheelDirection(Transform wheelTransform, float steerAngle)
    {
        Vector3 temp = wheelTransform.localEulerAngles;
        temp.y = steerAngle - wheelTransform.localEulerAngles.z;
        wheelTransform.localEulerAngles = temp;
    }
}
