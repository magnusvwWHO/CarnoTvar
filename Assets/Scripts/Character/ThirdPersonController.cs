using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonController : MonoBehaviour
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

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;

        private Vector3 velocity;
        
        private AnimationController animDriver;


        void Awake()
        {
            cc = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();

            // берём экшены по именам из PlayerControls
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            sprintAction = playerInput.actions["Sprint"];
            
            animDriver = GetComponent<AnimationController>();
        }

        void OnEnable()
        {
            moveAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
        }

        void OnDisable()
        {
            moveAction.Disable();
            jumpAction.Disable();
            sprintAction.Disable();
        }

        void Update()
        {
            if (!cameraTransform) return;

            // Ground
            if (cc.isGrounded && velocity.y < 0f)
                velocity.y = -2f;

            // Input
            Vector2 mv = moveAction.ReadValue<Vector2>();
            Vector3 inputDir = new Vector3(mv.x, 0f, mv.y);
            bool sprint = sprintAction.IsPressed();
            float speed = sprint ? sprintSpeed : moveSpeed;

            Vector3 moveDirWorld = Vector3.zero;

            // Move relative to camera
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

            // Jump
            if (cc.isGrounded && jumpAction.WasPressedThisFrame())
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // если ты сделал TriggerJump через драйвер:
                var animDriver = GetComponent<AnimationController>();
                if (animDriver) animDriver.TriggerJump();
            }

            // Gravity
            velocity.y += gravity * Time.deltaTime;

            // ONE move per frame (горизонталь + вертикаль вместе)
            Vector3 motion = moveDirWorld * speed + Vector3.up * velocity.y;
            cc.Move(motion * Time.deltaTime);
        }
    }
}