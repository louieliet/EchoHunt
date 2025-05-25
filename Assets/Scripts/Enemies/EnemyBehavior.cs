using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum ZombieState { Idle, Alert, Investigate, Chase, Searching }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour, ICapturable
{
    public Transform target;
    public float attackDistance;
    public float visionRange = 10f;
    public float visionAngle = 90f;

    public Transform EyesReference;

    private NavMeshAgent agent;
    private float m_Distance;
    public ZombieState currentState { get; private set; }

    [Header("Events")]
    public UnityEvent OnPlayerSpotted;
    public UnityEvent OnPlayerLost;

    [Header("AI Configuration")]
    [SerializeField] private float detectionUpdateRate = 0.2f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float investigationTime = 5f;
    [SerializeField] private float searchTime = 8f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float alertDecayTime = 3f;

    [Header("Memory System")]
    [SerializeField] private float memoryDuration = 10f;
    [SerializeField] private float predictionStrength = 0.5f;

    [Header("Communication")]
    [SerializeField] private float communicationRange = 15f;
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Hearing")]
    [SerializeField] private float hearingRange = 8f;
    [SerializeField] private LayerMask soundObstacleLayers;

    private PlayerMovement _playerMovement;
    private EnemyAnimator _enemyAnimator;
    private bool isZombieAwake;

    private float stunTimer;

    // Memory and AI enhancement variables
    private Vector3 lastKnownPlayerPosition;
    private float lastPlayerSeenTime;
    private float alertLevel; // 0-1, increases with detection
    private Vector3 investigationTarget;
    private float stateTimer;
    private bool hasMemoryOfPlayer;

    // Optimization variables
    private Coroutine detectionCoroutine;
    private bool canSeePlayerCached;
    private bool canHearPlayerCached;
    private float lastDetectionUpdate;

    private void Start()
    {
        isZombieAwake = false;
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        target = GameManager.player;
        _playerMovement = target.GetComponent<PlayerMovement>();
        _enemyAnimator = GetComponent<EnemyAnimator>();

        // Initialize AI variables
        alertLevel = 0f;
        hasMemoryOfPlayer = false;
        lastKnownPlayerPosition = Vector3.zero;

        StageBuilder.instance.OnLevelBuild += ResetZombie;
        GameManager.instance.OnQTEExit += Stun;
    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEExit -= Stun;

        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
        }
    }

    private void Stun(Transform initiator)
    {
        stunTimer = stunDuration;
        transform.forward = (target.position - transform.position).normalized;

        // Stop detection coroutine while stunned
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
            detectionCoroutine = null;
        }
    }

    private void ResetZombie()
    {
        GameManager.CreatedZombie();

        transform.position = StageBuilder.instance.GetRandomPositionAtMaze();
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

    private void Update()
    {
        if (!isZombieAwake) return;

        // Handle stun timer
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f && detectionCoroutine == null)
            {
                // Restart detection after stun ends
                detectionCoroutine = StartCoroutine(DetectionCoroutine());
            }
            return;
        }

        // Update timers
        stateTimer += Time.deltaTime;

        // Decay alert level over time
        if (alertLevel > 0f && !canSeePlayerCached)
        {
            alertLevel -= Time.deltaTime / alertDecayTime;
            alertLevel = Mathf.Clamp01(alertLevel);
        }

        // State machine logic
        HandleStateMachine();
    }

    private IEnumerator DetectionCoroutine()
    {
        while (isZombieAwake && stunTimer <= 0f)
        {
            UpdateDetection();
            yield return new WaitForSeconds(detectionUpdateRate);
        }
    }

    private void UpdateDetection()
    {
        m_Distance = Vector3.Distance(target.position, transform.position);
        canSeePlayerCached = CanSeePlayer();
        canHearPlayerCached = CanHearPlayer();
        lastDetectionUpdate = Time.time;

        // Update memory system
        if (canSeePlayerCached)
        {
            UpdatePlayerMemory();
        }
    }

    private void UpdatePlayerMemory()
    {
        lastKnownPlayerPosition = target.position;
        lastPlayerSeenTime = Time.time;
        hasMemoryOfPlayer = true;
        alertLevel = 1f; // Max alert when seeing player
    }

    private void HandleStateMachine()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                HandleIdleState();
                break;

            case ZombieState.Alert:
                HandleAlertState();
                break;

            case ZombieState.Investigate:
                HandleInvestigateState();
                break;

            case ZombieState.Chase:
                HandleChaseState();
                break;

            case ZombieState.Searching:
                HandleSearchingState();
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
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // Visualización del campo en el Editor
    private void OnDrawGizmosSelected()
    {
        // Vision range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Hearing range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        // Communication range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, communicationRange);

        // Attack distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Current state visualization
        if (Application.isPlaying)
        {
            // Last known player position
            if (hasMemoryOfPlayer)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(lastKnownPlayerPosition, 1f);
                Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
            }

            // Investigation target
            if (currentState == ZombieState.Investigate || currentState == ZombieState.Alert)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
                Gizmos.DrawWireSphere(investigationTarget, 0.5f);
            }

            // Alert level visualization
            Gizmos.color = Color.Lerp(Color.white, Color.red, alertLevel);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * alertLevel);
        }
    }

    public void Capture(Vector3 pos)
    {
        isZombieAwake = false;
        agent.enabled = false;

        GameManager.CaptureZombie();

        transform.position = pos;

        _enemyAnimator.Capture();
    }

    private void HandleIdleState()
    {
        _enemyAnimator.StateChange(currentState);
        agent.isStopped = true;

        if (canSeePlayerCached)
        {
            ChangeState(ZombieState.Chase);
            OnPlayerSpotted.Invoke();
            AlertNearbyEnemies();
        }
        else if (canHearPlayerCached)
        {
            investigationTarget = target.position;
            ChangeState(ZombieState.Alert);
        }
        else if (hasMemoryOfPlayer && Time.time - lastPlayerSeenTime < memoryDuration)
        {
            investigationTarget = lastKnownPlayerPosition;
            ChangeState(ZombieState.Investigate);
        }
    }

    private void HandleAlertState()
    {
        _enemyAnimator.StateChange(currentState);

        RotateTowards(target.position);

        if (canSeePlayerCached)
        {
            ChangeState(ZombieState.Chase);
            OnPlayerSpotted.Invoke();
            AlertNearbyEnemies();
        }
        else if (!canHearPlayerCached)
        {
            if (stateTimer > alertDecayTime)
            {
                investigationTarget = target.position;
                ChangeState(ZombieState.Investigate);
            }
        }
    }

    private void HandleInvestigateState()
    {
        _enemyAnimator.StateChange(currentState);

        if (canSeePlayerCached)
        {
            ChangeState(ZombieState.Chase);
            OnPlayerSpotted.Invoke();
            AlertNearbyEnemies();
            return;
        }

        // Move towards investigation target
        agent.isStopped = false;
        agent.destination = investigationTarget;

        // Check if reached investigation point
        if (Vector3.Distance(transform.position, investigationTarget) < 2f)
        {
            ChangeState(ZombieState.Searching);
        }
        else if (stateTimer > investigationTime)
        {
            ChangeState(ZombieState.Idle);
            OnPlayerLost.Invoke();
        }
    }

    private void HandleChaseState()
    {
        _enemyAnimator.StateChange(currentState);

        if (m_Distance <= attackDistance)
        {
            agent.isStopped = true;
            RegularZombieQTE.EnterRegularZombieQTE(EyesReference);
        }
        else
        {
            agent.isStopped = false;

            // Predict player movement for smarter chasing
            Vector3 targetPosition = PredictPlayerPosition();
            agent.destination = targetPosition;
        }

        if (!canSeePlayerCached && !canHearPlayerCached)
        {
            if (hasMemoryOfPlayer)
            {
                investigationTarget = lastKnownPlayerPosition;
                ChangeState(ZombieState.Investigate);
            }
            else
            {
                ChangeState(ZombieState.Searching);
            }
        }
    }

    private void HandleSearchingState()
    {
        _enemyAnimator.StateChange(currentState);

        if (canSeePlayerCached)
        {
            ChangeState(ZombieState.Chase);
            OnPlayerSpotted.Invoke();
            AlertNearbyEnemies();
            return;
        }

        // Search around the area
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 5f;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y;

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.destination = hit.position;
            }
        }

        if (stateTimer > searchTime)
        {
            ChangeState(ZombieState.Idle);
            OnPlayerLost.Invoke();
        }
    }

    private void ChangeState(ZombieState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            stateTimer = 0f;
        }
    }

    private Vector3 PredictPlayerPosition()
    {
        if (!_playerMovement) return target.position;

        // Get player velocity
        Vector3 playerVelocity = Vector3.zero;

        // Try to get player velocity if available
        if (_playerMovement.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            playerVelocity = rb.linearVelocity;
        }

        // Predict where player will be
        float timeToReach = m_Distance / agent.speed;
        Vector3 predictedPosition = target.position + (playerVelocity * timeToReach * predictionStrength);

        return predictedPosition;
    }

    private void AlertNearbyEnemies()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, communicationRange, enemyLayerMask);

        foreach (Collider enemyCollider in nearbyEnemies)
        {
            if (enemyCollider == this.GetComponent<Collider>()) continue;

            EnemyBehavior otherEnemy = enemyCollider.GetComponent<EnemyBehavior>();
            if (otherEnemy != null)
            {
                otherEnemy.ReceiveAlert(lastKnownPlayerPosition);
            }
        }
    }

    public void ReceiveAlert(Vector3 playerPosition)
    {
        if (currentState == ZombieState.Idle || currentState == ZombieState.Alert)
        {
            lastKnownPlayerPosition = playerPosition;
            hasMemoryOfPlayer = true;
            alertLevel = 0.7f;
            investigationTarget = playerPosition;
            ChangeState(ZombieState.Investigate);
        }
    }
}