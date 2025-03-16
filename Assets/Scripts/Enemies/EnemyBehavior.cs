using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour
{
    public Transform target; // Referencia al jugador
    public float attackDistance; // Distancia a la que el enemigo ataca
    public float visionRange = 10f; // Rango de visión del enemigo
    public float visionAngle = 90f; // Ángulo de visión del enemigo

    private NavMeshAgent agent;
    private float m_Distance;
    private Vector3 startingPoint;
    private bool canSeePlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startingPoint = transform.position; // Guarda la posición inicial del enemigo
    }

    void Update()
    {
        m_Distance = Vector3.Distance(target.position, transform.position);

        // Verifica si el jugador está dentro del rango de visión
        canSeePlayer = CanSeePlayer();

        if (canSeePlayer && m_Distance <= attackDistance)
        {
            // Si el jugador está cerca, detén al enemigo
            agent.isStopped = true;
        }
        else if (canSeePlayer)
        {
            // Si el jugador está dentro del rango de visión, persíguelo
            agent.isStopped = false;
            agent.destination = target.position;
        }
        else
        {
            // Si no ve al jugador, regresa al punto inicial
            agent.isStopped = false;
            agent.destination = startingPoint;
        }
    }

    private bool CanSeePlayer()
    {
        // Verifica si el jugador está dentro del rango de visión
        if (m_Distance > visionRange)
        {
            return false;
        }

        // Verifica si el jugador está dentro del ángulo de visión
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        if (angleToTarget > visionAngle / 2)
        {
            return false;
        }

        // Verifica si hay obstáculos entre el enemigo y el jugador
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToTarget, out hit, visionRange))
        {
            if (hit.transform == target)
            {
                return true; // El enemigo ve al jugador
            }
        }

        return false; // No ve al jugador
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el rango de visión en el editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Dibuja el ángulo de visión en el editor
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}