using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class SuperiorEnemyBehavior : EnemyBehavior
{
    [Header("Superior Enemy Settings")]
    [SerializeField] private float runThresholdDistance = 8f; // Distancia para decidir si corre o camina
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 7f;

    private Vector3 lastHeardSoundPosition;
    private bool shouldRunToSound = false;

    protected override void Start()
    {
        base.Start();
        // Superior hearing
        hearingRange = 20f; // Puedes ajustar este valor en el Inspector
        // Ciego
        visionRange = 0f;
        visionAngle = 0f;
    }

    public override void Capture(Transform pos)
    {
        return;
    }

    protected override bool CanSeePlayer()
    {
        // Siempre es ciego
        return false;
    }

    protected override void UpdateDetection()
    {
        // Solo usa el oído
        canHearPlayerCached = CanHearPlayer();
        lastDetectionUpdate = Time.time;

        if (canHearPlayerCached)
        {
            lastHeardSoundPosition = target.position;
            float distance = Vector3.Distance(transform.position, lastHeardSoundPosition);
            shouldRunToSound = distance > runThresholdDistance;
            UpdatePlayerMemory();
        }
    }

    protected override void HandleIdleState()
    {
        _enemyAnimator.StateChange(currentState);
        agent.isStopped = true;

        if (canHearPlayerCached)
        {
            investigationTarget = lastHeardSoundPosition;
            ChangeState(ZombieState.Alert);
        }
        else if (hasMemoryOfPlayer && Time.time - lastPlayerSeenTime < memoryDuration)
        {
            investigationTarget = lastKnownPlayerPosition;
            ChangeState(ZombieState.Investigate);
        }
    }

    protected override void HandleAlertState()
    {
        _enemyAnimator.StateChange(currentState);

        RotateTowards(investigationTarget);

        if (!canHearPlayerCached)
        {
            if (stateTimer > alertDecayTime)
            {
                ChangeState(ZombieState.Investigate);
            }
        }
    }

    protected override void HandleInvestigateState()
    {
        _enemyAnimator.StateChange(currentState);

        // Decide velocidad según shouldRunToSound
        agent.speed = shouldRunToSound ? runSpeed : walkSpeed;

        // Mueve hacia el punto del sonido
        agent.isStopped = false;
        agent.destination = investigationTarget;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // Ataca si está cerca del punto de sonido o cerca del jugador
        if (distanceToPlayer < attackDistance)
        {
            agent.isStopped = true;
            RegularZombieQTE.EnterRegularZombieQTE(transform); // Usa el transform del Superior
        }
        else if (stateTimer > investigationTime)
        {
            ChangeState(ZombieState.Idle);
        }
    }

    protected override void ResetZombie()
    {
        // Igual que el base, pero sin GameManager.CreatedZombie();
        transform.position = StageBuilder.instance.GetRandomMazePosition();
        isZombieAwake = true;
        agent.enabled = true;
        currentState = ZombieState.Idle;

        // Reset AI state
        alertLevel = 0f;
        hasMemoryOfPlayer = false;
        stateTimer = 0f;

        // Start optimized detection system
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
        }
        detectionCoroutine = StartCoroutine(DetectionCoroutine());
    }

    public override void Hear(Vector3 position)
    {
        investigationTarget = position;
        lastHeardSoundPosition = position;
        float distance = Vector3.Distance(transform.position, lastHeardSoundPosition);
        shouldRunToSound = distance > runThresholdDistance;

        ChangeState(ZombieState.Investigate);
    }
}