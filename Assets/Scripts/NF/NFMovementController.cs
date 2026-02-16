using UnityEngine;
using UnityEngine.InputSystem;

namespace NF
{
    public class NfMovementController : MonoBehaviour
    {
        [Header("Links")] [Tooltip("Обычно сюда Main Camera (Transform).")]
        public Transform cameraTransform;

        [Header("Movement")] public float moveSpeed = 5.5f;
        public float sprintSpeed = 8.0f;
        public float rotationSpeed = 12f;

        [Header("Jump & Gravity")] public float gravity = -18f;
        public float jumpHeight = 1.2f;

        private CharacterController _cc;
        private PlayerInput _playerInput;

        private InputAction _moveAction;
        private InputAction _sitAction;
        private InputAction _sprintAction;

        private Vector3 _velocity;
        
        private NFAnimationController _animDriver;


        void Awake()
        {
            _animDriver = GetComponent<NFAnimationController>();
            _cc = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();

            // берём экшены по именам из PlayerControls
            _moveAction = _playerInput.actions["Move"];
            _sitAction = _playerInput.actions["Sit"];
            _sprintAction = _playerInput.actions["Sprint"];
        }

        void OnEnable()
        {
            _moveAction.Enable();
            _sitAction.Enable();
            _sprintAction.Enable();
        }

        void OnDisable()
        {
            _moveAction.Disable();
            _sitAction.Disable();
            _sprintAction.Disable();
        }

        void Update()
        {
            if (!cameraTransform) return;

            // Ground
            if (_cc.isGrounded && _velocity.y < 0f)
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
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDirWorld = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                // rotate character
                Quaternion targetRot = Quaternion.LookRotation(moveDirWorld, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Sit
            if (_cc.isGrounded && _sitAction.WasPressedThisFrame())
            {
                if (_animDriver) _animDriver.TriggerSit();
            }

            // Gravity
            _velocity.y += gravity * Time.deltaTime;

            // ONE move per frame (горизонталь + вертикаль вместе)
            Vector3 motion = moveDirWorld * speed + Vector3.up * _velocity.y;
            _cc.Move(motion * Time.deltaTime);
        }
    }
}