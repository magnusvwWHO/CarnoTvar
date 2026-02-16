using NF;
using UnityEngine;

public class NFAnimationController : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Sit = Animator.StringToHash("Sit");
    public CharacterController characterController;
    public Animator animator;

    [Header("Take from movement script")]
    public NfMovementController movement; // перетащи сюда компонент движения

    [Header("Tuning")]
    public float speedSmooth = 12f;

    private float _smoothedSpeed01;

    void Awake()
    {
        if (!characterController) characterController = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!movement) movement = GetComponent<NfMovementController>();
    }

    void Update()
    {
        if (!characterController || !animator || !movement) return;

        // горизонтальная скорость
        Vector3 v = characterController.velocity;
        v.y = 0f;
        float speed = v.magnitude;

        // нормализация в 0..1:
        // 0 = стоим, moveSpeed = примерно 0.5, sprintSpeed = 1.0
        float speed01;
        if (speed <= 0.01f) speed01 = 0f;
        else if (speed < movement.moveSpeed) speed01 = Mathf.InverseLerp(0f, movement.moveSpeed, speed) * 0.5f;
        else speed01 = Mathf.InverseLerp(movement.moveSpeed, movement.sprintSpeed, speed) * 0.5f + 0.5f;

        _smoothedSpeed01 = Mathf.Lerp(_smoothedSpeed01, speed01, speedSmooth * Time.deltaTime);

        animator.SetFloat(Speed, _smoothedSpeed01);
    }

    public void TriggerSit() => animator.SetTrigger(Sit);
}
