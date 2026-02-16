using UnityEngine;
using UnityEngine.InputSystem;

public class NFCameraController : MonoBehaviour
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
        
        [Header("Zoom")]
        public float minDistance = 2.0f;
        public float maxDistance = 7.0f;
        public float zoomSpeed = 0.02f;

        [Header("Smoothing")]
        public float positionSmooth = 12f;
        public float rotationSmooth = 12f;
        
        [Header("Collision")]
        public LayerMask collisionMask = ~0;   // по умолчанию всё
        public float sphereRadius = 0.25f;     // “толщина” камеры
        public float collisionOffset = 0.1f;   // отступ от стены

        private PlayerInput _playerInput;
        private InputAction _lookAction;
        private InputAction _zoomAction;

        private float _yaw;
        private float _pitch;

        void Awake()
        {
            _playerInput = GetComponentInParent<PlayerInput>();
            if (!_playerInput) _playerInput = FindFirstObjectByType<PlayerInput>();

            if (_playerInput)
            {
                _lookAction = _playerInput.actions["Look"];
                _zoomAction = _playerInput.actions["Zoom"];
            }
        }

        void OnEnable()
        {
            if (_lookAction != null) _lookAction.Enable();
            if (_zoomAction != null) _zoomAction.Enable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDisable()
        {
            if (_lookAction != null) _lookAction.Disable();
            if (_zoomAction != null) _zoomAction.Enable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;
        }

        void LateUpdate()
        {
            if (!target || _lookAction == null) return;

            Vector2 look = _lookAction.ReadValue<Vector2>();
            
            _zoomAction = _playerInput.actions["Zoom"];

            // Определяем устройство: мышь или геймпад
            float sens = (_playerInput != null && _playerInput.currentControlScheme == "Gamepad")
                ? sensitivityGamepad
                : sensitivityMouse;

            _yaw += look.x * sens;
            _pitch -= look.y * sens;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            
            Vector2 scroll = _zoomAction.ReadValue<Vector2>();
            distance = Mathf.Clamp(distance - scroll.y * zoomSpeed, minDistance, maxDistance);

            Quaternion desiredRot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * height; // точка, откуда “смотрим” (у головы)
            Vector3 desiredPos = pivot - (desiredRot * Vector3.forward) * distance;

            // --- Camera collision ---
            Vector3 dir = desiredPos - pivot;
            float dist = dir.magnitude;
            if (dist > 0.001f)
            {
                dir /= dist;

                if (Physics.SphereCast(pivot, sphereRadius, dir, out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float safeDist = Mathf.Max(hit.distance - collisionOffset, 0.05f);
                    desiredPos = pivot + dir * safeDist;
                }
            }
            
            transform.position = Vector3.Lerp(transform.position, desiredPos, positionSmooth * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);

        }
    }
