using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    private Rigidbody rigidbody;
    private float verticalInput = 0.0f;
    private float horizontalInput = 0.0f;
    private bool isBreaking = false;
    private float steeringAngle = 0.0f;

    [Header("Settings")]
    [SerializeField] private float motorForce = 100.0f;
    [SerializeField] private float breakForce = 100.0f;
    [SerializeField] private float maxSteeringAngle = 45.0f;

    [Header("Wheels")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerCar"), LayerMask.NameToLayer("PlayerCar"));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();

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
}
