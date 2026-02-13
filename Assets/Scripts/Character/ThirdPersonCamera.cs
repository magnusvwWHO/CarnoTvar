using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;  // Player

        [Header("Orbit")]
        public float distance = 4.5f;
        public float height = 1.6f;

        [Header("Look")]
        public float sensitivityMouse = 0.12f;   // мышь обычно "резче"
        public float sensitivityGamepad = 2.0f;  // геймпад обычно "мягче"
        public float minPitch = -35f;
        public float maxPitch = 70f;

        [Header("Smoothing")]
        public float positionSmooth = 12f;
        public float rotationSmooth = 12f;

        private PlayerInput playerInput;
        private InputAction lookAction;

        private float yaw;
        private float pitch;

        void Awake()
        {
            playerInput = GetComponentInParent<PlayerInput>();
            if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();

            if (playerInput)
                lookAction = playerInput.actions["Look"];
        }

        void OnEnable()
        {
            if (lookAction != null) lookAction.Enable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDisable()
        {
            if (lookAction != null) lookAction.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }

        void LateUpdate()
        {
            if (!target || lookAction == null) return;

            Vector2 look = lookAction.ReadValue<Vector2>();

            // Определяем устройство: мышь или геймпад
            float sens = (playerInput != null && playerInput.currentControlScheme == "Gamepad")
                ? sensitivityGamepad
                : sensitivityMouse;

            yaw += look.x * sens;
            pitch -= look.y * sens;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion desiredRot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPos = target.position + Vector3.up * height - (desiredRot * Vector3.forward) * distance;

            transform.position = Vector3.Lerp(transform.position, desiredPos, positionSmooth * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);
        }
    }
}
