using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public static void Spawn(Vector3 _position, Quaternion _rotation)
    {
        GameObject prefab = Resources.Load<GameObject>("Car");
        GameObject newCar = Instantiate(prefab, _position, _rotation);
    }

    private Rigidbody rigidbody;
    private float verticalInput = 0.0f;
    private float horizontalInput = 0.0f;
    private bool isBreaking = false;
    private float steeringAngle = 0.0f;

    [Header("Settings")]
    [SerializeField] private float motorForce = 100.0f;
    [SerializeField] private float breakForce = 100.0f;
    [SerializeField] private float maxSteeringAngle = 45.0f;

    [SerializeField] private float xRotLock = 30.0f;
    [SerializeField] private float zRotLock = 30.0f;
    [SerializeField] private Transform centerOfMass;

    [Header("Wheels")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centerOfMass.localPosition;

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerCar"), LayerMask.NameToLayer("PlayerCar"));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();

        // Lock rotation max
        if (transform.rotation.eulerAngles.x > 180.0f && transform.rotation.eulerAngles.x < 360.0f - xRotLock)
        {
            Debug.Log($"Locking -X: {transform.rotation.eulerAngles.x}");
            transform.rotation = Quaternion.Euler(-xRotLock, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        if (transform.rotation.eulerAngles.x < 180.0f && transform.rotation.eulerAngles.x > xRotLock)
        {
            Debug.Log($"Locking X: {transform.rotation.eulerAngles.x}");
            transform.rotation = Quaternion.Euler(xRotLock, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        if (transform.rotation.eulerAngles.z > 180.0f && transform.rotation.eulerAngles.z < 360.0f - zRotLock)
        {
            Debug.Log($"Locking -Z: {transform.rotation.eulerAngles.z}");
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -zRotLock);
        }
        if (transform.rotation.eulerAngles.z < 180.0f && transform.rotation.eulerAngles.z > zRotLock)
        {
            Debug.Log($"Locking Z: {transform.rotation.eulerAngles.z}");
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, zRotLock);
        }
    }
    private void GetInput()
    {
        verticalInput = 0.0f;
        horizontalInput = 0.0f;

        verticalInput += InputManager.Instance.IsBindPressed("Move_Forward") ? 1.0f : 0.0f;
        verticalInput -= InputManager.Instance.IsBindPressed("Move_Backward") ? 1.0f : 0.0f;

        horizontalInput += InputManager.Instance.IsBindPressed("Move_Right") ? 1.0f : 0.0f;
        horizontalInput -= InputManager.Instance.IsBindPressed("Move_Left") ? 1.0f : 0.0f;

        isBreaking = InputManager.Instance.IsBindPressed("Roll");
    }
    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        float currentBreakForce = isBreaking ? breakForce : 0.0f;

        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        backLeftWheelCollider.brakeTorque = currentBreakForce;
        backRightWheelCollider.brakeTorque = currentBreakForce;
        
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
}
