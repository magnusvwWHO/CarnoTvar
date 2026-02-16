using Character;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarnoMovementController : MonoBehaviour
{
        [Header("Links")]
        [Tooltip("Обычно сюда Main Camera (Transform).")]
        public Transform cameraTransform;

        [Header("Movement")]
        public float moveSpeed = 5.5f;
        public float sprintSpeed = 8.0f;
        public float rotationSpeed = 12f;

        [Header("Jump & Gravity")]
        public float gravity = -18f;
        public float jumpHeight = 1.2f;

        private CharacterController cc;
        private PlayerInput playerInput;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _attackAction;
        private InputAction _altAction;

        private Vector3 _velocity;

        private CarnoAnimationController _driver;


        void Awake()
        {
            _driver = GetComponent<CarnoAnimationController>();
            cc = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();

            // берём экшены по именам из PlayerControls
            _moveAction = playerInput.actions["Move"];
            _jumpAction = playerInput.actions["Jump"];
            _sprintAction = playerInput.actions["Sprint"];
            _attackAction =  playerInput.actions["Attack"];
            _altAction = playerInput.actions["AltAction"];
        }

        void OnEnable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _sprintAction.Enable();
            _attackAction.Enable();
            _altAction.Enable();
        }

        void OnDisable()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
            _sprintAction.Disable();
            _attackAction.Disable();
            _altAction.Disable();
        }

        void Update()
        {
            if (!cameraTransform) return;

            // Ground
            if (cc.isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;

            // Input
            Vector2 mv = _moveAction.ReadValue<Vector2>();
            Vector3 inputDir = new Vector3(mv.x, 0f, mv.y);
            bool sprint = _sprintAction.IsPressed();
            float speed = sprint ? sprintSpeed : moveSpeed;

            Vector3 moveDirWorld = Vector3.zero;

            //Move relative to camera
            if (inputDir.sqrMagnitude > 0.001f)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f; camRight.y = 0f;
                camForward.Normalize(); camRight.Normalize();
            
                moveDirWorld = (camForward * inputDir.z + camRight * inputDir.x).normalized;
            
                // rotate character
                Quaternion targetRot = Quaternion.LookRotation(moveDirWorld, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Gravity
            _velocity.y += gravity * Time.deltaTime;
            
            //Bite
            if (_altAction.WasReleasedThisFrame())
                _driver.SetAltAction(false);
            
            if (_altAction.WasPressedThisFrame())
                _driver.SetAltAction(true);

            if (_attackAction.WasPressedThisFrame())
            {
                _driver.TriggerBite();
            }

            // ONE move per frame (горизонталь + вертикаль вместе)
            Vector3 motion = moveDirWorld * speed + Vector3.up * _velocity.y;
            cc.Move(motion * Time.deltaTime);
        }
    }
