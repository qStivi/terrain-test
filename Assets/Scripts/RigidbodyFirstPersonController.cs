using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyFirstPersonController : MonoBehaviour
{
    public Camera cam;
    public MovementSettings movementSettings = new MovementSettings();
    public MouseLook mouseLook = new MouseLook();
    public AdvancedSettings advancedSettings = new AdvancedSettings();
    private CapsuleCollider _mCapsule;
    private Vector3 _mGroundContactNormal;
    private bool _mJump, _mPreviouslyGrounded;


    private Rigidbody _mRigidBody;


    public bool Grounded { get; private set; }

    public bool Jumping { get; private set; }


    private void Start()
    {
        _mRigidBody = GetComponent<Rigidbody>();
        _mCapsule = GetComponent<CapsuleCollider>();
        mouseLook.Init(transform, cam.transform);
    }


    private void Update()
    {
        RotateView();

        if (Input.GetButtonDown("Jump") && !_mJump) _mJump = true;
    }


    private void FixedUpdate()
    {
        GroundCheck();
        var input = GetInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || Grounded))
        {
            // always move along the camera forward as it is the direction that it being aimed at
            var transform1 = cam.transform;
            var desiredMove = transform1.forward * input.y + transform1.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, _mGroundContactNormal).normalized;

            desiredMove.x *= movementSettings.currentTargetSpeed;
            desiredMove.z *= movementSettings.currentTargetSpeed;
            desiredMove.y *= movementSettings.currentTargetSpeed;
            if (_mRigidBody.velocity.sqrMagnitude <
                movementSettings.currentTargetSpeed * movementSettings.currentTargetSpeed)
                _mRigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
        }

        if (Grounded)
        {
            _mRigidBody.drag = 5f;

            if (_mJump)
            {
                _mRigidBody.drag = 0f;
                var velocity = _mRigidBody.velocity;
                velocity = new Vector3(velocity.x, 0f, velocity.z);
                _mRigidBody.velocity = velocity;
                _mRigidBody.AddForce(new Vector3(0f, movementSettings.jumpForce, 0f), ForceMode.Impulse);
                Jumping = true;
            }

            if (!Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && _mRigidBody.velocity.magnitude < 1f) _mRigidBody.Sleep();
        }
        else
        {
            _mRigidBody.drag = 0f;
            if (_mPreviouslyGrounded && !Jumping) StickToGroundHelper();
        }

        _mJump = false;
    }


    private float SlopeMultiplier()
    {
        var angle = Vector3.Angle(_mGroundContactNormal, Vector3.up);
        return movementSettings.slopeCurveModifier.Evaluate(angle);
    }


    private void StickToGroundHelper()
    {
        if (!Physics.SphereCast(transform.position, _mCapsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out var hitInfo,
            _mCapsule.height / 2f - _mCapsule.radius +
            advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) return;
        if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            _mRigidBody.velocity = Vector3.ProjectOnPlane(_mRigidBody.velocity, hitInfo.normal);
    }


    private Vector2 GetInput()
    {
        var input = new Vector2
        {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }


    private void RotateView()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        var transform1 = transform;
        var oldYRotation = transform1.eulerAngles.y;

        mouseLook.LookRotation(transform1, cam.transform);

        if (Grounded || advancedSettings.airControl)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            var velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            _mRigidBody.velocity = velRotation * _mRigidBody.velocity;
        }
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
    {
        _mPreviouslyGrounded = Grounded;
        if (Physics.SphereCast(transform.position, _mCapsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out var hitInfo,
            _mCapsule.height / 2f - _mCapsule.radius + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Grounded = true;
            _mGroundContactNormal = hitInfo.normal;
        }
        else
        {
            Grounded = false;
            _mGroundContactNormal = Vector3.up;
        }

        if (!_mPreviouslyGrounded && Grounded && Jumping) Jumping = false;
    }

    [Serializable]
    public class MovementSettings
    {
        public float forwardSpeed = 8.0f; // Speed when walking forward
        public float backwardSpeed = 4.0f; // Speed when walking backwards
        public float strafeSpeed = 4.0f; // Speed when walking sideways
        public float runMultiplier = 2.0f; // Speed when sprinting
        public KeyCode runKey = KeyCode.LeftShift;
        public float jumpForce = 30f;
        public AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector] public float currentTargetSpeed = 8f;

#if !MOBILE_INPUT
        public bool Running { get; private set; }
#endif

        public void UpdateDesiredTargetSpeed(Vector2 input)
        {
            if (input == Vector2.zero) return;
            if (input.x > 0 || input.x < 0)
                //strafe
                currentTargetSpeed = strafeSpeed;
            if (input.y < 0)
                //backwards
                currentTargetSpeed = backwardSpeed;
            if (input.y > 0)
                //forwards
                //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                currentTargetSpeed = forwardSpeed;
#if !MOBILE_INPUT
            if (Input.GetKey(runKey))
            {
                currentTargetSpeed *= runMultiplier;
                Running = true;
            }
            else
            {
                Running = false;
            }
#endif
        }

#if !MOBILE_INPUT
#endif
    }


    [Serializable]
    public class AdvancedSettings
    {
        public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
        public bool airControl; // can the user control the direction that is being moved in the air

        [Tooltip("set it to 0.1 or more if you get stuck in wall")]
        public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
    }
}
