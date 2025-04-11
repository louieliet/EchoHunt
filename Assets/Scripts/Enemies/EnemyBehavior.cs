using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum ZombieState { Idle, Alert, Chase }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour
{
    public Transform target;
    public float attackDistance;
    public float visionRange = 10f;
    public float visionAngle = 90f;

    public Transform EyesReference;

    private NavMeshAgent agent;
    private float m_Distance;
    private ZombieState currentState = ZombieState.Idle;

    [Header("Events")]
    public UnityEvent OnPlayerSpotted;
    public UnityEvent OnPlayerLost;

    [Header("Hearing")]
    [SerializeField] private float hearingRange = 8f;
    [SerializeField] private LayerMask soundObstacleLayers;

    private PlayerMovement _playerMovement;
    private EnemyAnimator _enemyAnimator;
    private bool isZombieAwake;

    private void Start()
    {
        isZombieAwake = false;
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        target = GameObject.FindWithTag("Player").transform;
        _playerMovement = target.GetComponent<PlayerMovement>();
        _enemyAnimator = GetComponent<EnemyAnimator>();

        StageBuilder.instance.OnLevelBuild += ResetZombie;
    }

    private void ResetZombie()
    {
        transform.position = StageBuilder.instance.GetRandomPositionAtMaze();
        isZombieAwake = true;
        agent.enabled = true;
        currentState = ZombieState.Idle;
    }

    private void Update()
    {
        if (!isZombieAwake) return;

        m_Distance = Vector3.Distance(target.position, transform.position);
        bool canSeePlayer = CanSeePlayer();
        bool canHearPlayer = CanHearPlayer();

        switch (currentState)
        {
            case ZombieState.Idle:
                _enemyAnimator.SetWalking(false); // Animación Idle

                if (canHearPlayer && !canSeePlayer)
                {
                    currentState = ZombieState.Alert;
                }
                else if (canSeePlayer)
                {
                    currentState = ZombieState.Chase;
                    OnPlayerSpotted.Invoke();
                }
                break;

            case ZombieState.Alert:
                _enemyAnimator.SetWalking(true); // Animación de "caminar alerta" (puede ser la misma que caminar normal)

                RotateTowards(target.position);

                if (canSeePlayer)
                {
                    currentState = ZombieState.Chase;
                    OnPlayerSpotted.Invoke();
                }
                else if (!canHearPlayer)
                {
                    currentState = ZombieState.Idle;
                    OnPlayerLost.Invoke();
                }
                break;

            case ZombieState.Chase:
                _enemyAnimator.SetWalking(true); // Animación de caminar/persiguiendo

                if (m_Distance <= attackDistance)
                {
                    agent.isStopped = true;
                }
                else
                {
                    agent.isStopped = false;
                    agent.destination = target.position;
                }

                if (!canSeePlayer && !canHearPlayer)
                {
                    currentState = ZombieState.Idle;
                    OnPlayerLost.Invoke();
                }
                break;
        }
    }

    private bool CanSeePlayer()
    {
        if (m_Distance > visionRange)
            return false;

        Vector3 directionToTarget = (target.position - EyesReference.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        if (angleToTarget > visionAngle / 2)
            return false;

        if (Physics.Raycast(EyesReference.position, directionToTarget, out RaycastHit hit, visionRange))
        {
            return hit.transform == target;
        }
        return false;
    }

    private bool CanHearPlayer()
    {
        if (!_playerMovement.IsMakingNoise) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        float playerNoiseRadius = _playerMovement.GetCurrentNoiseRadius();

        // Aquí usamos transform.position para mejorar la detección
        if (distanceToPlayer <= hearingRange + playerNoiseRadius)
        {
            Vector3 directionToPlayer = (target.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit,
                hearingRange + playerNoiseRadius, soundObstacleLayers))
            {
                return hit.transform == target;
            }
            return true;
        }
        return false;
    }

    // Método para hacer rotar el zombie hacia una posición de forma suave
    private void RotateTowards(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }
    }

    // Visualización del campo en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}