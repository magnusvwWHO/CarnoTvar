using UnityEngine;
using UnityEngine.InputSystem;

namespace Carno
{
    public class HeadTurnDriver : MonoBehaviour
    {
        private static readonly int HeadTurningAngle = Animator.StringToHash("HeadTurningAngle");

        [Header("Refs")]
        [SerializeField] private Animator animator;

        [Header("Tuning")]
        [SerializeField] private float sensitivity = 0.01f; // мышь: подбирай
        [SerializeField] private float smooth = 10f;

        private float target;
        private float current;

        // Это вызовет PlayerInput, если назовёшь action "LookX"
        public void OnLookX(InputAction.CallbackContext ctx)
        {
            float x = ctx.ReadValue<float>();   // для Axis
            // Если у тебя mouse delta как Vector2, см. вариант ниже
            target = Mathf.Clamp(x * sensitivity, -1f, 1f);
            Debug.Log(target);
        }

        // Если action "Look" = Vector2 (mouse delta)
        public void OnLook(InputAction.CallbackContext ctx)
        {
            Vector2 delta = ctx.ReadValue<Vector2>();
            target = Mathf.Clamp(delta.x * sensitivity, -1f, 1f);
        }

        private void Update()
        {
            current = Mathf.Lerp(current, target, Time.deltaTime * smooth);
            animator.SetFloat(HeadTurningAngle, current);
        }
    }
}
