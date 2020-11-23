using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentAnimation : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    public Vector2 smoothDeltaPosition = Vector2.zero;
    public Vector2 velocity = Vector2.zero;
    public Vector3 previousPosition;

    void Start()
    {
        if(!animator)
            animator = GetComponent<Animator>();
        if(!agent)
            agent = GetComponent<NavMeshAgent>();

        previousPosition = transform.position;
    }

    void Update()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - previousPosition;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);

        velocity = new Vector2(dx, dy) / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.1f && agent.remainingDistance > agent.radius;

        animator.SetFloat("velx", velocity.x / agent.speed);
        animator.SetFloat("vely", velocity.y / agent.speed);

        previousPosition = transform.position;
    }
}
