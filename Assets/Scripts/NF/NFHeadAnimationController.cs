using UnityEngine;

namespace NF
{
    public class NFHeadAnimationController : MonoBehaviour
    {
        private static readonly int HeadHorizontal = Animator.StringToHash("HeadHorizontal");
        private static readonly int HeadVertical = Animator.StringToHash("HeadVertical");

        [SerializeField] private Animator animator;
        [SerializeField] private Transform body;
        [SerializeField] private Transform cameraT;

        [Header("Horizontal (yaw)")] [SerializeField]
        private float responseYaw = 140f;

        [SerializeField] private float deadZoneYawDeg = 2f;
        [SerializeField] private float frontWindow = 35f;
        [Range(0f, 1f)] [SerializeField] private float frontGlanceAmount = 0.2f;

        [Header("Vertical (pitch)")] [SerializeField]
        private float responsePitch = 60f;

        [SerializeField] private float deadZonePitchDeg = 1.5f;
        [SerializeField] private float maxPitchUp = 35f;
        [SerializeField] private float maxPitchDown = 25f;

        [Header("Smoothing")] [SerializeField] private float smoothTime = 0.08f;

        private float hCurrent, vCurrent;
        private float hVel, vVel;

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

            if (abs < deadZoneYawDeg) delta = 0f;

            float normal = Mathf.Clamp(delta / responseYaw, -1f, 1f);

            float frontStart = 180f - frontWindow;
            float frontT = Mathf.InverseLerp(frontStart, 180f, abs); // 0..1
            float front = Mathf.Sign(delta == 0f ? 1f : delta) * (frontGlanceAmount * frontT);

            float hTarget = Mathf.Lerp(normal, front, frontT);

            hCurrent = Mathf.SmoothDamp(hCurrent, hTarget, ref hVel, smoothTime);

            // =========================
            // VERTICAL (pitch) — добавили
            // =========================
            // Pitch направления камеры относительно горизонта: вверх +, вниз -
            float pitch = Mathf.Atan2(fwd3.y, Mathf.Sqrt(fwd3.x * fwd3.x + fwd3.z * fwd3.z)) * Mathf.Rad2Deg;

            if (Mathf.Abs(pitch) < deadZonePitchDeg) pitch = 0f;

            // Ограничим вверх/вниз (обычно у головы асимметрия)
            pitch = Mathf.Clamp(pitch, -maxPitchDown, maxPitchUp);

            float vTarget = Mathf.Clamp(pitch / responsePitch, -1f, 1f);

            vCurrent = Mathf.SmoothDamp(vCurrent, vTarget, ref vVel, smoothTime);

            // =========================
            // В Animator (- потому что у тебя было -current)
            // =========================
            animator.SetFloat(HeadHorizontal, hCurrent);
            animator.SetFloat(HeadVertical, vCurrent);
        }
    }
}