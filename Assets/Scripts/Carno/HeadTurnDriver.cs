using UnityEngine;
using UnityEngine.InputSystem;

namespace Carno
{
    public class HeadTurnDriver : MonoBehaviour
    {
        private static readonly int HeadTurningAngle = Animator.StringToHash("HeadTurningAngle");

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

        [Range(0f, 1f)]
        [SerializeField] private float frontGlanceAmount = 0.2f;

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.08f; // меньше = быстрее, больше = плавнее

        private float current;
        private float vel;

        void Reset() => body = transform;

        void Update()
        {
            if (!animator || !body || !cameraT) return;

            // Берём только yaw направления взгляда камеры
            Vector3 fwd = cameraT.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) return;
            fwd.Normalize();

            float bodyYaw = body.eulerAngles.y;
            float camYaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;

            float delta = Mathf.DeltaAngle(bodyYaw, camYaw); // [-180..180]
            float abs = Mathf.Abs(delta);

            // Deadzone, чтобы не дрожало около 0
            if (abs < deadZoneDeg) delta = 0f;

            // Основная реакция: мягкая, потому что делим на responseYaw, а не maxHeadYaw
            float normal = Mathf.Clamp(delta / responseYaw, -1f, 1f);

            // Если камера "спереди" (угол близок к 180) — делаем небольшое поглядывание, а не попытку 180°
            float frontStart = 180f - frontWindow;
            float frontT = Mathf.InverseLerp(frontStart, 180f, abs); // 0..1
            float front = Mathf.Sign(delta == 0f ? 1f : delta) * (frontGlanceAmount * frontT);

            float target = Mathf.Lerp(normal, front, frontT);

            // Стабильное сглаживание без рывков
            current = Mathf.SmoothDamp(current, target, ref vel, smoothTime);
            animator.SetFloat(HeadTurningAngle, -current);
        }
    }
}
