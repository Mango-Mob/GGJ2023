using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public static void Spawn(Vector3 _position, Quaternion _rotation)
    {
        GameObject prefab = Resources.Load<GameObject>("Player");
        GameObject newCar = Instantiate(prefab, _position, _rotation);
    }

    private Rigidbody rigidbody;
    private Collider mainCollider;
    private float verticalInput = 0.0f;
    private float horizontalInput = 0.0f;
    private Vector2 tiltInput = Vector2.zero;
    private bool isBraking = false;
    private bool isNOS = false;
    private bool isJumping = false;
    private bool isSwimming = false;
    private float steeringAngle = 0.0f;

    [SerializeField] private Camera playerCamera;

    [Header("Settings")]
    [SerializeField] private float motorForce = 100.0f;
    [SerializeField] private float brakeForce = 100.0f;
    [SerializeField] private float maxSteeringAngle = 45.0f;
    [SerializeField] private float nosMult = 100.0f;
    [SerializeField] private float jumpVelocity = 5.0f;
    [SerializeField] private float airTiltForce = 5.0f;

    [SerializeField] private float xRotLock = 30.0f;
    [SerializeField] private float zRotLock = 30.0f;
    [SerializeField] private Transform centerOfMass;

    [Header("Swimming")]
    [SerializeField] private float waterFloatVelocity = 2.5f;
    [SerializeField] private float waterFloatDepth = 0.5f;
    [SerializeField] private float waterDamping = 1.0f;
    [SerializeField] private float swimAcceleration = 10.0f;

    [Header("Wheels")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform;
    [SerializeField] private Transform backRightWheelTransform;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<BoxCollider>();
        rigidbody.centerOfMass = centerOfMass.localPosition;

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerCar"), LayerMask.NameToLayer("PlayerCar"));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        HandleJumping();
        HandleSwimming();
        UpdateWheels();


    }
    private void GetInput()
    {
        verticalInput = 0.0f;
        horizontalInput = 0.0f;

        // Forward movement
        if (InputManager.Instance.IsBindPressed("Move_Forward") || InputManager.Instance.IsBindPressed("Move_Backward"))
        {
            verticalInput += InputManager.Instance.IsBindPressed("Move_Forward") ? 1.0f : 0.0f;
            verticalInput -= InputManager.Instance.IsBindPressed("Move_Backward") ? 1.0f : 0.0f;
        }
        else
        {
            verticalInput += InputManager.Instance.GetRightTrigger(0);
            verticalInput -= InputManager.Instance.GetLeftTrigger(0);
        }

        // Side movement
        if (InputManager.Instance.IsBindPressed("Move_Right") || InputManager.Instance.IsBindPressed("Move_Left"))
        {
            horizontalInput += InputManager.Instance.IsBindPressed("Move_Right") ? 1.0f : 0.0f;
            horizontalInput -= InputManager.Instance.IsBindPressed("Move_Left") ? 1.0f : 0.0f;
        }
        else
        {
            horizontalInput = InputManager.Instance.GetBindStick("Move").x;
        }

        Debug.Log($"RPM: {frontLeftWheelCollider.rpm}, Input: {verticalInput}, {Mathf.Sign(verticalInput) != Mathf.Sign(frontLeftWheelCollider.rpm)}");
        float angle = Vector3.Angle(transform.forward, rigidbody.velocity);
        bool forceBrake = verticalInput != 0.0f && (angle > 90.0f && Mathf.Sign(verticalInput) == 1.0f || angle < 90.0f && Mathf.Sign(verticalInput) == -1.0f);
        
        isBraking = InputManager.Instance.IsBindPressed("Roll") || forceBrake;
        isNOS = InputManager.Instance.IsBindPressed("Boost");
        isJumping = InputManager.Instance.IsBindPressed("Jump");
    }
    private void HandleMotor()
    {
        float currentNosForce = isNOS ? nosMult : 1.0f;

        frontLeftWheelCollider.motorTorque = verticalInput * motorForce * currentNosForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce * currentNosForce;

        float currentBreakForce = isBraking ? brakeForce : 0.0f;

        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        backLeftWheelCollider.brakeTorque = currentBreakForce;
        backRightWheelCollider.brakeTorque = currentBreakForce;
        
    }
    private void HandleJumping()
    {
        bool isGrounded = frontLeftWheelCollider.isGrounded || frontRightWheelCollider.isGrounded || backLeftWheelCollider.isGrounded || backRightWheelCollider.isGrounded;

        if (isJumping && isGrounded)
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);
            rigidbody.angularVelocity = new Vector3(0.0f, rigidbody.angularVelocity.y, 0.0f);
        }
        tiltInput = Vector2.zero;
        tiltInput.x += InputManager.Instance.IsBindPressed("Move_Right") ? 1.0f : 0.0f;
        tiltInput.x -= InputManager.Instance.IsBindPressed("Move_Left") ? 1.0f : 0.0f;
        tiltInput.y += InputManager.Instance.IsBindPressed("Move_Forward") ? 1.0f : 0.0f;
        tiltInput.y -= InputManager.Instance.IsBindPressed("Move_Backward") ? 1.0f : 0.0f;

        if (tiltInput.magnitude == 0.0f)
            tiltInput = InputManager.Instance.GetBindStick("Move");

        if (!isGrounded)
        {
            rigidbody.AddRelativeTorque(new Vector3(tiltInput.y * airTiltForce * Time.fixedDeltaTime, 0.0f, -tiltInput.x * airTiltForce * Time.fixedDeltaTime), ForceMode.Acceleration);
        }


        // Lock rotation max
        if (transform.rotation.eulerAngles.x > 180.0f && transform.rotation.eulerAngles.x < 360.0f - xRotLock)
        {
            Debug.Log($"Locking -X: {transform.rotation.eulerAngles.x}");
            rigidbody.angularVelocity = new Vector3(0.0f, rigidbody.angularVelocity.y, rigidbody.angularVelocity.z);
            transform.rotation = Quaternion.Euler(-xRotLock, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        if (transform.rotation.eulerAngles.x < 180.0f && transform.rotation.eulerAngles.x > xRotLock)
        {
            Debug.Log($"Locking X: {transform.rotation.eulerAngles.x}");
            rigidbody.angularVelocity = new Vector3(0.0f, rigidbody.angularVelocity.y, rigidbody.angularVelocity.z);
            transform.rotation = Quaternion.Euler(xRotLock, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        if (transform.rotation.eulerAngles.z > 180.0f && transform.rotation.eulerAngles.z < 360.0f - zRotLock)
        {
            Debug.Log($"Locking -Z: {transform.rotation.eulerAngles.z}");
            rigidbody.angularVelocity = new Vector3(rigidbody.angularVelocity.x, rigidbody.angularVelocity.y, 0.0f);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -zRotLock);
        }
        if (transform.rotation.eulerAngles.z < 180.0f && transform.rotation.eulerAngles.z > zRotLock)
        {
            Debug.Log($"Locking Z: {transform.rotation.eulerAngles.z}");
            rigidbody.angularVelocity = new Vector3(rigidbody.angularVelocity.x, rigidbody.angularVelocity.y, 0.0f);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, zRotLock);
        }
    }
    private void HandleSwimming()
    {
        isSwimming = transform.position.y < waterFloatDepth;

        if (isSwimming)
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, waterFloatVelocity, rigidbody.velocity.z);

            Vector3 cameraForward = playerCamera.transform.forward;
            cameraForward.y = 0.0f;

            Vector3 cameraRight = playerCamera.transform.right;
            cameraRight.y = 0.0f;

            Vector3 swimDirection = cameraForward * tiltInput.y + cameraRight * tiltInput.x;

            rigidbody.AddForce(swimAcceleration * swimDirection, ForceMode.Acceleration);
        }
    }
    private void HandleSteering()
    {
        steeringAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = steeringAngle;
        frontRightWheelCollider.steerAngle = steeringAngle;
    }
    public void HaltWheels()
    {
        frontLeftWheelCollider.brakeTorque = Mathf.Infinity;
        frontRightWheelCollider.brakeTorque = Mathf.Infinity;
        backLeftWheelCollider.brakeTorque = Mathf.Infinity;
        backRightWheelCollider.brakeTorque = Mathf.Infinity;
    }
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(backLeftWheelCollider, backLeftWheelTransform);
        UpdateSingleWheel(backRightWheelCollider, backRightWheelTransform);
    }
    private void UpdateSingleWheel(WheelCollider _collider, Transform _transform)
    {
        Vector3 pos;
        Quaternion rot;
        _collider.GetWorldPose(out pos, out rot);
        _transform.rotation = rot;
        _transform.position = pos;
    }
}
