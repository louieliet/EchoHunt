using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour
{
    public Transform target;
    public float attackDistance;

    private NavMeshAgent agent;
    private float m_Distance;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        m_Distance = Vector3.Distance(target.position, transform.position);
        if (m_Distance <= attackDistance)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            agent.destination = target.position;
        }
    }

}
