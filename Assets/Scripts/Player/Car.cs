using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.VFX;
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
    [SerializeField] private float jumpVelocity = 5.0f;
    [SerializeField] private float waterJumpMult = 2.0f;
    [SerializeField] private float airTiltForce = 5.0f;
    [SerializeField] private float startBoost = 5.0f;
    [SerializeField] private float speedometerScale = 5.0f;

    [SerializeField] private float xRotLock = 30.0f;
    [SerializeField] private float zRotLock = 30.0f;
    [SerializeField] private Transform centerOfMass;

    [Header("Boost")]
    [SerializeField] private float nosMult = 100.0f;
    [SerializeField] private float nosRechargeRate = 0.3f;
    private float nosCharge = 3.0f;
    private float maxNosCharge = 3.0f;

    [Header("Pulling")]
    [SerializeField] private float pullTimeReq = 1.5f;
    private float pullTimer = 0.0f;

    [Header("Swimming")]
    [SerializeField] private float waterFloatVelocity = 2.5f;
    [SerializeField] private float waterFloatDepth = 0.5f;
    [SerializeField] private float waterDamping = 1.0f;
    [SerializeField] private float swimAcceleration = 10.0f;
    [SerializeField] private float swimTorqueMult = 10.0f;
    [SerializeField] private Animator floatsAnimator;

    [Header("Wheels")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform;
    [SerializeField] private Transform backRightWheelTransform;

    [Header("Harpoon")]
    [SerializeField] private GameObject harpoonLauncher;
    [SerializeField] private GameObject harpoon;
    [SerializeField] private float harpoonRange = 30.0f;
    [SerializeField] private float harpoonLockonRange = 5.0f;
    [SerializeField] private float harpoonSpeed = 10.0f;
    [SerializeField] private float harpoonSpring = 10.0f;
    [SerializeField] private LayerMask harpoonTargetMask;
    [SerializeField] private LineRenderer ropeRenderer;

    [Header("VFX")]
    [SerializeField] private VisualEffect tailSmoke;
    [SerializeField] private WorldToCanvas targetUI;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator cameraShake;

    [Header("Audio")]
    [SerializeField] private SoloAudioAgent idleAudio;
    [SerializeField] private SoloAudioAgent driveAudio;
    private MultiAudioAgent audioAgent;
    [SerializeField] public float driveAudioSmoothTime = 0.3f;
    private float driveAudioVelocity = 0.0f;
    private float driveAudioLerp = 0.0f;

    private float idleMaxVolume = 0.0f;
    private float driveMaxVolume = 0.0f;

    // Upgrades
    [HideInInspector] public float accelMult = 1.0f;
    [HideInInspector] public float accelMultMin = 0.8f;
     
    [HideInInspector] public float nosCooldownMult = 1.0f;
    [HideInInspector] public float nosCooldownMultMin = 0.8f;
     
    [HideInInspector] public float impactMult = 1.0f;
    [HideInInspector] public float impactMultMin = 0.8f;
     
    [HideInInspector] public float steeringAngleMult = 1.0f;
    [HideInInspector] public float steeringAngleMultMin = 0.8f;
    
    [HideInInspector] public float jumpMult = 1.0f;
    [HideInInspector] public float jumpMultMin = 0.8f;
    
    [HideInInspector] public float swimSpeedMult = 1.0f;
    [HideInInspector] public float swimSpeedMultMin = 0.8f;
    public void ShakeCamera()
    {
        cameraShake.SetTrigger("Shake");
    }
    public void ApplyStat(Modifier.Stat.Modifies to, float value)
    {
        switch (to)
        {
            case Modifier.Stat.Modifies.Accel:
                accelMult += value;

                if (accelMult < accelMultMin)
                    accelMult = accelMultMin;
                break;
            case Modifier.Stat.Modifies.CoolDown:
                nosCooldownMult += value;

                if (nosCooldownMult < nosCooldownMultMin)
                    nosCooldownMult = nosCooldownMultMin;
                break;
            case Modifier.Stat.Modifies.Impact:
                impactMult += value;

                if (impactMult < impactMultMin)
                    impactMult = impactMultMin;
                break;
            case Modifier.Stat.Modifies.Steering:
                steeringAngleMult += value;

                if (steeringAngleMult < steeringAngleMultMin)
                    steeringAngleMult = steeringAngleMultMin;
                break;
            case Modifier.Stat.Modifies.Jump:
                jumpMult += value;

                if (jumpMult < jumpMultMin)
                    jumpMult = jumpMultMin;
                break;
            case Modifier.Stat.Modifies.Swim_Speed:
                swimSpeedMult += value;

                if (swimSpeedMult < swimSpeedMultMin)
                    swimSpeedMult = swimSpeedMultMin;
                break;
            default:
                break;
        }
    }

    private Tree currentTarget;
    private GameObject harpoonProjectile;
    private Vector3 harpoonOrigin = Vector3.zero;

    private Vector3 harpoonStartPos = Vector3.zero;
    private Vector3 harpoonStartRot = Vector3.zero;
    private bool hooked = false;
    private bool isHarpoonTravelling = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<BoxCollider>();
        audioAgent = GetComponent<MultiAudioAgent>();
        rigidbody.centerOfMass = centerOfMass.localPosition;

        harpoonOrigin = harpoon.transform.localPosition;
        GameManager.Instance.m_player = gameObject;
        GameManager.Instance.m_activeCamera = playerCamera;

        nosCharge = maxNosCharge;

        idleMaxVolume = idleAudio.localVolume;
        driveMaxVolume = driveAudio.localVolume;

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerCar"), LayerMask.NameToLayer("PlayerCar"));
    }
    public void NotifyDestroyed(Tree _tree)
    {
        if (currentTarget == _tree)
            StopHarpoon();
    }
    public Tree GetHookedTree()
    {
        if (hooked && currentTarget != null)
            return currentTarget;
        else
            return null;
    }

    private void Update()
    {
        if(GameManager.Instance.time_scale == 0)
        {
            //We are paused!
            return;
        }

        // Driver audio
        float enginePower = Mathf.Abs(verticalInput);
        driveAudioLerp = Mathf.SmoothDamp(driveAudioLerp, enginePower, ref driveAudioVelocity, driveAudioSmoothTime);

        driveAudio.localVolume = Mathf.Clamp01((driveAudioLerp) * idleMaxVolume);
        driveAudio.isMuted = driveAudioLerp < 0.01f;

        idleAudio.localVolume = Mathf.Clamp01((1.0f - driveAudioLerp * 0.5f) * driveMaxVolume);
        idleAudio.isMuted = driveAudioLerp > 0.99f;

        // Boost
        nosCharge += Time.deltaTime * nosRechargeRate * nosCooldownMult;
        nosCharge = Mathf.Clamp(nosCharge, 0.0f, maxNosCharge);
        if (nosCharge >= 1.0f && InputManager.Instance.IsBindDown("Boost"))
        {
            audioAgent.Play("Van Boost 1");
            rigidbody.AddForce(transform.forward * nosMult, ForceMode.Impulse);
            nosCharge -= 1.0f;
        }

        // Beep beep
        if (InputManager.Instance.IsBindDown("Jump"))
        {
            audioAgent.Play("Van Horn");
        }

        PlayerHUD.Instance.charges.UpdateValue(nosCharge);

        Vector3 playerSpeed = rigidbody.velocity;
        playerSpeed.y = 0.0f;

        PlayerHUD.Instance.speed.value = (playerSpeed.magnitude / 100.0f);


        ropeRenderer.enabled = hooked;
        if (!hooked)
        {
            //harpoonJoint.connectedAnchor = transform.position;

            // Aim
            float harpoonRotation = playerCamera.transform.eulerAngles.y;
            harpoonLauncher.transform.eulerAngles = new Vector3(0.0f, harpoonRotation, 0.0f);

            var hits = Physics.SphereCastAll(harpoonLauncher.transform.position, harpoonLockonRange, harpoonLauncher.transform.forward, harpoonRange, harpoonTargetMask);
            Debug.Log(hits.Length);


            float closestDistance = Mathf.Infinity;
            Tree closestTarget = null;
            foreach (var hit in hits)
            {
                Tree tree = hit.collider.GetComponent<Tree>();
                if (tree == null)
                    continue;

                float distance = Vector3.Distance(harpoonLauncher.transform.position, tree.transform.position);
                if (closestTarget == null || distance < closestDistance)
                {
                    closestTarget = tree;
                    closestDistance = distance;
                }
            }

            currentTarget = closestTarget;

        }
        else
        {
            Vector3 harpoonOffset = harpoon.transform.position - harpoonLauncher.transform.position;
            harpoonOffset.y = 0.0f;

            harpoonLauncher.transform.forward = harpoonOffset;

            if (isHarpoonTravelling)
                harpoon.transform.eulerAngles = harpoonStartRot;
            else
                harpoon.transform.eulerAngles = currentTarget.transform.rotation * harpoonStartRot;

            if (!isHarpoonTravelling)
                harpoon.transform.position = currentTarget.centreOfMass.position;
        }

        if (InputManager.Instance.IsBindDown("Fire"))
        {
            if (!hooked && currentTarget)
            {
                StartCoroutine(FireHarpoon());
            }
            else
            {
                if (currentTarget)
                    StopHarpoon();
            }
        }

        targetUI.GetComponent<Image>().enabled = !hooked && currentTarget != null;

        if (currentTarget)
            targetUI.m_anchorTransform = currentTarget.centreOfMass;
        else
            targetUI.m_anchorTransform = null;

    }
    private void StopHarpoon()
    {
        currentTarget = null;
        harpoon.transform.localPosition = harpoonOrigin;
        harpoon.transform.localRotation = Quaternion.identity;
        hooked = false;

        audioAgent.Play("Harpoon Reload");
    }
    IEnumerator FireHarpoon()
    {
        if (currentTarget != null)
        {
            muzzleFlash.Play();

            hooked = true;
            harpoonStartPos = harpoon.transform.position;
            harpoonStartRot = harpoon.transform.eulerAngles;

            audioAgent.Play("Harpoon Shoot");

            isHarpoonTravelling = true;
            float travelLerp = 0.0f;
            float distance = Vector3.Distance(harpoonStartPos, currentTarget.centreOfMass.position);

            while (travelLerp < 1.0f)
            {
                travelLerp += Time.deltaTime * harpoonSpeed / distance;
                harpoon.transform.position = Vector3.Lerp(harpoonStartPos, currentTarget.centreOfMass.position, travelLerp);

                yield return new WaitForEndOfFrame();
            }

            isHarpoonTravelling = false;
            audioAgent.Play("Harpoon Hit");
        }
    }

    private void OnDrawGizmos()
    {
        if (currentTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentTarget.centreOfMass.position, 0.5f);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.time_scale == 0)
        {
            verticalInput = 0.0f;
            horizontalInput = 0.0f;
            tiltInput = Vector2.zero;

            //We are paused!
            return;
        }

        GetInput();
        HandleMotor();
        HandleSteering();
        HandleJumping();
        HandleSwimming();
        HandleHarpoon();
        UpdateWheels();

        if (verticalInput > 0.0f)
            tailSmoke.Play();
        else
            tailSmoke.Stop();
    }
    private void HandleHarpoon()
    {
        if (hooked && !isHarpoonTravelling)
        {
            Vector3 hookDifference = rigidbody.worldCenterOfMass - currentTarget.transform.position;
            if (hookDifference.magnitude > harpoonRange)
            {
                currentTarget.GetComponent<Rigidbody>().AddForce((hookDifference.magnitude - harpoonRange) * harpoonSpring * hookDifference.normalized);
                rigidbody.AddForce((hookDifference.magnitude - harpoonRange) * harpoonSpring * -hookDifference.normalized);

                pullTimer += Time.fixedDeltaTime * impactMult;
                if (pullTimer >= pullTimeReq)
                {
                    currentTarget.Uproot();
                    pullTimer = 0.0f;
                }
            }
            else
            {
                pullTimer = 0.0f;
            }
        }
        else
        {
            pullTimer = 0.0f;
        }
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

        float angle = Vector3.Angle(transform.forward, rigidbody.velocity);
        bool forceBrake = verticalInput != 0.0f && (angle > 90.0f && Mathf.Sign(verticalInput) == 1.0f || angle < 90.0f && Mathf.Sign(verticalInput) == -1.0f);
        
        isBraking = InputManager.Instance.IsBindPressed("Roll") || forceBrake;
        //isNOS = InputManager.Instance.IsBindPressed("Boost");
        isJumping = InputManager.Instance.IsBindPressed("Jump");
    }
    private void HandleMotor()
    {
        float currentNosForce = isNOS ? nosMult : 1.0f;

        if (rigidbody.velocity.magnitude < 1.0f)
        {
            rigidbody.AddForce(transform.forward * startBoost * Time.fixedDeltaTime * verticalInput, ForceMode.VelocityChange);
        }

        frontLeftWheelCollider.motorTorque = verticalInput * motorForce * currentNosForce * accelMult;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce * currentNosForce * accelMult;

        float currentBreakForce = isBraking ? brakeForce : 0.0f;

        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        backLeftWheelCollider.brakeTorque = currentBreakForce;
        backRightWheelCollider.brakeTorque = currentBreakForce;
        
    }

    private void HandleJumping()
    {
        bool isGrounded = frontLeftWheelCollider.isGrounded || frontRightWheelCollider.isGrounded || backLeftWheelCollider.isGrounded || backRightWheelCollider.isGrounded;

        if (isJumping && (isGrounded || isSwimming))
        {
            audioAgent.PlayOnce("Van Jump");

            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
            rigidbody.velocity += (isSwimming ? Vector3.up : transform.up) * jumpVelocity * (isSwimming ? waterJumpMult : 1.0f) * jumpMult;
            rigidbody.angularVelocity = new Vector3(0.0f, rigidbody.angularVelocity.y, 0.0f);
        }
        tiltInput = Vector2.zero;
        tiltInput.x += InputManager.Instance.IsBindPressed("Move_Right") ? 1.0f : 0.0f;
        tiltInput.x -= InputManager.Instance.IsBindPressed("Move_Left") ? 1.0f : 0.0f;
        tiltInput.y += InputManager.Instance.IsBindPressed("Move_Forward") ? 1.0f : 0.0f;
        tiltInput.y -= InputManager.Instance.IsBindPressed("Move_Backward") ? 1.0f : 0.0f;

        if (tiltInput.magnitude == 0.0f)
            tiltInput = InputManager.Instance.GetBindStick("Move");

        if (!isGrounded && !isSwimming)
        {
            rigidbody.AddRelativeTorque(new Vector3(tiltInput.y * airTiltForce * Time.fixedDeltaTime, 0.0f, -tiltInput.x * airTiltForce * Time.fixedDeltaTime), ForceMode.Acceleration);
        }


        // Lock rotation max
        if (transform.rotation.eulerAngles.x > 180.0f && transform.rotation.eulerAngles.x < 360.0f - xRotLock)
        {
            //Debug.Log($"Locking -X: {transform.rotation.eulerAngles.x}");
            rigidbody.angularVelocity = new Vector3(0.0f, rigidbody.angularVelocity.y, rigidbody.angularVelocity.z);
            transform.rotation = Quaternion.Euler(-xRotLock, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        if (transform.rotation.eulerAngles.x < 180.0f && transform.rotation.eulerAngles.x > xRotLock)
        {
            //Debug.Log($"Locking X: {transform.rotation.eulerAngles.x}");
            rigidbody.angularVelocity = new Vector3(0.0f, rigidbody.angularVelocity.y, rigidbody.angularVelocity.z);
            transform.rotation = Quaternion.Euler(xRotLock, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        if (transform.rotation.eulerAngles.z > 180.0f && transform.rotation.eulerAngles.z < 360.0f - zRotLock)
        {
            //Debug.Log($"Locking -Z: {transform.rotation.eulerAngles.z}");
            rigidbody.angularVelocity = new Vector3(rigidbody.angularVelocity.x, rigidbody.angularVelocity.y, 0.0f);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -zRotLock);
        }
        if (transform.rotation.eulerAngles.z < 180.0f && transform.rotation.eulerAngles.z > zRotLock)
        {
            //Debug.Log($"Locking Z: {transform.rotation.eulerAngles.z}");
            rigidbody.angularVelocity = new Vector3(rigidbody.angularVelocity.x, rigidbody.angularVelocity.y, 0.0f);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, zRotLock);
        }
    }
    private void HandleSwimming()
    {
        isSwimming = transform.position.y < waterFloatDepth;

        floatsAnimator.SetBool("inflate", isSwimming);

        rigidbody.drag = isSwimming ? waterDamping : 0.0f;
        rigidbody.angularDrag = isSwimming ? waterDamping : 0.05f;

        if (isSwimming)
        {
            rigidbody.AddForce(Vector3.up * waterFloatVelocity * Mathf.Abs(transform.position.y - waterFloatDepth) * Time.fixedDeltaTime, ForceMode.Acceleration);

            Vector3 cameraForward = playerCamera.transform.forward;
            cameraForward.y = 0.0f;

            Vector3 cameraRight = playerCamera.transform.right;
            cameraRight.y = 0.0f;

            Vector3 swimDirection = cameraForward * tiltInput.y + cameraRight * tiltInput.x;

            rigidbody.AddForce(swimAcceleration * swimDirection * jumpMult, ForceMode.Acceleration);

            float xDifference = 0.0f;
            if (transform.rotation.eulerAngles.x > 180.0f)
            {
                xDifference -= transform.rotation.eulerAngles.x - 360.0f;
            }
            else if (transform.rotation.eulerAngles.x <= 180.0f)
            {
                xDifference = transform.rotation.eulerAngles.x;
            }

            float zDifference = 0.0f;
            if (transform.rotation.eulerAngles.z > 180.0f)
            {
                zDifference -= transform.rotation.eulerAngles.z - 360.0f;
            }
            else if (transform.rotation.eulerAngles.z <= 180.0f)
            {
                zDifference = transform.rotation.eulerAngles.z;
            }

            Debug.Log($"{xDifference}, {zDifference}");
            Vector3 swimTorque = new Vector3(-xDifference, 0.0f, zDifference);
            rigidbody.AddRelativeTorque(swimTorque * swimTorqueMult * Time.fixedDeltaTime, ForceMode.Acceleration);

            if (swimDirection.magnitude > 0.0f)
            {
                rigidbody.AddTorque(Vector3.up * -Vector3.SignedAngle(swimDirection, transform.forward, Vector3.up) * swimTorqueMult * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }
    private void HandleSteering()
    {
        steeringAngle = maxSteeringAngle * horizontalInput * steeringAngleMult;
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
