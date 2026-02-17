using UnityEngine;
using UnityEngine.InputSystem;

namespace Carno
{
    public class HeadTurnDriver : MonoBehaviour
    {
        private static readonly int HeadTurningAngle = Animator.StringToHash("HeadTurningAngle");
        private static readonly int HeadTurningVertical = Animator.StringToHash("HeadTurningVertical");

        [Header("Refs")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform body;       // поворот тела (yaw)
        [SerializeField] private Transform cameraT;    // камера (берём forward)

        [Header("Response")]
        [Tooltip("Угол (в градусах), при котором HeadTurn станет 1/-1. Сделай БОЛЬШЕ, чтобы голова не упиралась рано.")]
        [SerializeField] private float responseYaw = 120f;

        [Tooltip("Мёртвая зона вокруг 0°, чтобы не дёргалось.")]
        [SerializeField] private float deadZoneDeg = 2f;

        [Header("Front glance (optional)")]
        [Tooltip("Окно около 180°, где вместо разворота на 180° делаем лёгкий 'взгляд'.")]
        [SerializeField] private float frontWindow = 35f;
        [SerializeField] private float maxPitchDown = 35f;
        [SerializeField] private float maxPitchUp = 40f;
        
        [Header("Vertical (pitch)")] [SerializeField]
        private float responsePitch = 60f;

        [Range(0f, 1f)]
        [SerializeField] private float frontGlanceAmount = 0.2f;

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.08f; // меньше = быстрее, больше = плавнее

        private float _currentH, _currentV;
        private float _velH, _velV;

        void Reset() => body = transform;

        void Update()
        {
            if (!animator || !body || !cameraT) return;

            // Берём направление взгляда камеры
            Vector3 fwd3 = cameraT.forward;
            if (fwd3.sqrMagnitude < 0.0001f) return;
            fwd3.Normalize();

            // =========================
            // HORIZONTAL (yaw) — твой код
            // =========================
            Vector3 fwd = fwd3;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) return;
            fwd.Normalize();

            float bodyYaw = body.eulerAngles.y;
            float camYaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;

            float delta = Mathf.DeltaAngle(bodyYaw, camYaw); // [-180..180]
            float abs = Mathf.Abs(delta);

            if (abs < deadZoneDeg) delta = 0f;

            float normal = Mathf.Clamp(delta / responseYaw, -1f, 1f);

            float frontStart = 180f - frontWindow;
            float frontT = Mathf.InverseLerp(frontStart, 180f, abs); // 0..1
            float front = Mathf.Sign(delta == 0f ? 1f : delta) * (frontGlanceAmount * frontT);

            float hTarget = Mathf.Lerp(normal, front, frontT);

            _currentH = Mathf.SmoothDamp(_currentH, hTarget, ref _velH, smoothTime);

            // =========================
            // VERTICAL (pitch) — добавили
            // =========================
            // Pitch направления камеры относительно горизонта: вверх +, вниз -
            float pitch = Mathf.Atan2(fwd3.y, Mathf.Sqrt(fwd3.x * fwd3.x + fwd3.z * fwd3.z)) * Mathf.Rad2Deg;

            if (Mathf.Abs(pitch) < deadZoneDeg) pitch = 0f;

            // Ограничим вверх/вниз (обычно у головы асимметрия)
            pitch = Mathf.Clamp(pitch, -maxPitchDown, maxPitchUp);

            float vTarget = Mathf.Clamp(pitch / responsePitch, -1f, 1f);

            _currentV = Mathf.SmoothDamp(_currentV, vTarget, ref _velV, smoothTime);

            // =========================
            // В Animator (- потому что у тебя было -current)
            // =========================
            animator.SetFloat(HeadTurningAngle, _currentH);
            animator.SetFloat(HeadTurningVertical, _currentV);
        }
    }
}
