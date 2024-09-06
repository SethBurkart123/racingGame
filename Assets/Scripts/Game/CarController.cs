using UnityEngine;
using Mirror;

public class RacingCarController : NetworkBehaviour
{
    public WheelCollider WheelFL, WheelFR, WheelRL, WheelRR;
    public Transform WheelFLtrans, WheelFRtrans, WheelRLtrans, WheelRRtrans;
    public Vector3 eulertest;
    public Transform centreofmass;

    [SyncVar]
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

    [SyncVar]
    private float currentSpeed;

    [SyncVar]
    public int CurrentLap = 0;

    [SyncVar]
    public bool HasFinished = false;

    [SyncVar(hook = nameof(OnControlsEnabledChanged))]
    private bool controlsEnabled = false;

    [Header("Camera Settings")]
    public GameObject cameraPrefab;
    private SmoothCarCamera cameraScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centreofmass.transform.localPosition;
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || !controlsEnabled) return;

        if (!braked)
        {
            WheelFL.brakeTorque = WheelFR.brakeTorque = WheelRL.brakeTorque = WheelRR.brakeTorque = 0;
        }

        float accelerationInput = Input.GetAxis("Vertical");
        float currentTorque = maxTorque * Mathf.Sign(accelerationInput) * Mathf.Pow(Mathf.Abs(accelerationInput), accelerationFactor);
        currentTorque *= Mathf.Lerp(1f, Mathf.Abs(accelerationInput), accelerationCurve);
        WheelRR.motorTorque = WheelRL.motorTorque = currentTorque;

        currentSpeed = rb.velocity.magnitude;

        float speedFactor = Mathf.Clamp01(currentSpeed / 100f);
        float currentSteeringAngle = Mathf.Lerp(maxSteeringAngle, minSteeringAngle, speedFactor * steeringReductionFactor);

        WheelFL.steerAngle = WheelFR.steerAngle = currentSteeringAngle * Input.GetAxis("Horizontal");

        CmdUpdateServerState(currentSpeed, WheelFL.steerAngle, braked);
    }

    void Update()
    {
        // log controls enabled
        Debug.Log("Controls Enabled: " + controlsEnabled);

        if (!isLocalPlayer || !controlsEnabled) return;

        HandBrake();
        
        RotateWheel(WheelFLtrans, WheelFL.rpm);
        RotateWheel(WheelFRtrans, WheelFR.rpm);
        RotateWheel(WheelRLtrans, WheelRL.rpm);
        RotateWheel(WheelRRtrans, WheelRR.rpm);

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

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        SetupCamera();
        RpcSetControlsEnabled(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (isLocalPlayer)
        {
            CmdRequestControlsEnabled();
        }
    }

    private void SetupCamera()
    {
        if (isLocalPlayer)
        {
            GameObject cameraObject = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
            cameraScript = cameraObject.GetComponent<SmoothCarCamera>();
            if (cameraScript != null)
            {
                cameraScript.target = this.transform;
                cameraScript.enabled = true;
            }
            else
            {
                Debug.LogError("SmoothCarCamera script not found on camera prefab!");
            }
        }
    }

    [ClientRpc]
    public void RpcSetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        if (isLocalPlayer && cameraScript != null)
        {
            cameraScript.enabled = enabled;
        }
    }

    private void OnControlsEnabledChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"Controls Enabled changed from {oldValue} to {newValue}");
    }

    [Command]
    void CmdUpdateServerState(float speed, float steerAngle, bool isBraked)
    {
        currentSpeed = speed;
        WheelFL.steerAngle = WheelFR.steerAngle = steerAngle;
        braked = isBraked;
        RpcUpdateClientsState(speed, steerAngle, isBraked);
    }

    [ClientRpc]
    void RpcUpdateClientsState(float speed, float steerAngle, bool isBraked)
    {
        if (!isLocalPlayer)
        {
            currentSpeed = speed;
            WheelFL.steerAngle = WheelFR.steerAngle = steerAngle;
            braked = isBraked;
        }
    }

    [Command]
    public void CmdCrossedFinishLine()
    {
        if (!HasFinished)
        {
            HasFinished = true;
            CurrentLap++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.CompareTag("FinishLine"))
        {
            CmdCrossedFinishLine();
        }
        else if (other.CompareTag("Checkpoint"))
        {
            // You can add checkpoint logic here if needed
        }
    }

    [Command]
    private void CmdRequestControlsEnabled()
    {
        RpcSetControlsEnabled(true);
    }
}