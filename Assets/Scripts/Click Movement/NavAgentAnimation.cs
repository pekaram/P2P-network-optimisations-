using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentAnimation : MonoBehaviour
{
    public enum State
    {
        Moving,
        MoveToSit,
        SitToMove,
        Sitting
    }

    [SerializeField]
    private State currentState = State.Moving;
    [SerializeField]
    private State targetState = State.Moving;

    public Animator animator;
    public NavMeshAgent agent;
    public Vector2 smoothDeltaPosition = Vector2.zero;
    public Vector2 velocity = Vector2.zero;
    public Vector3 previousPosition;
    public Transform hips;
    
    public Seat currentSeat;
    public Seat nextSeat;

    public Vector3 modelSittingOffset;
    public float sitProgress;

    public State CurrentState
    {
        get => currentState;

        // Uses set state to cycle around the states to the target from the current state
        set
        {
            switch (value)
            {
                case State.Moving:
                    switch (currentState)
                    {
                        case State.Moving:
                            currentState = State.Moving;
                            break;
                        case State.MoveToSit:
                            currentState = State.Sitting;
                            break;
                        case State.SitToMove:
                            currentState = State.Moving;
                            break;
                        case State.Sitting:
                            currentState = State.SitToMove;
                            break;
                    }
                    break;
                case State.Sitting:
                    switch (currentState)
                    {
                        case State.Moving:
                            currentState = State.MoveToSit;
                            break;
                        case State.MoveToSit:
                            currentState = State.Sitting;
                            break;
                        case State.SitToMove:
                            currentState = State.Moving;
                            break;
                        case State.Sitting:
                            currentState = State.Sitting;
                            break;
                    }
                    break;
            }
        }
    }

    public State TargetState { get => targetState; set => targetState = value; }

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
        switch (CurrentState)
        {
            case State.Moving:
                // Sets the navmesh-based anim
                MovementAnimRotation();

                if (agent.remainingDistance == 0.0f && !agent.pathPending)
                {
                    sitProgress = 0.0f;

                    currentSeat = nextSeat;
                    nextSeat = null;

                    CurrentState = TargetState;
                }

                break;

            case State.MoveToSit:

                agent.speed = 0;

                if (currentSeat.sittingType == Seat.SittingType.Teleport)
                {
                    animator.SetTrigger("force sit");
                    animator.SetBool("sit", true);

                    agent.transform.position = currentSeat.sittingPoint.position + Vector3.up * 0.05f;
                    agent.transform.rotation = currentSeat.sittingPoint.rotation;

                    CurrentState = State.Sitting;

                    break;
                }

                agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, currentSeat.navigationPoint.rotation, Time.deltaTime);
                agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, currentSeat.navigationPoint.rotation, Time.deltaTime * 150);

                if (agent.transform.rotation == currentSeat.navigationPoint.rotation)
                {
                    animator.SetBool("sit", true);
                    sitProgress = Mathf.MoveTowards(sitProgress, 1f, Time.deltaTime * 0.5f);
                    agent.transform.position = Vector3.Lerp(currentSeat.navigationPoint.position, currentSeat.sittingPoint.position, currentSeat.SitDownAnim.Evaluate(sitProgress)) + modelSittingOffset;

                    if (sitProgress == 1f)
                    {
                        CurrentState = State.Sitting;
                    }
                }

                break;

            case State.SitToMove:

                if (currentSeat.sittingType == Seat.SittingType.Teleport)
                {
                    animator.SetTrigger("force move");
                    animator.SetBool("sit", false);

                    agent.transform.position = currentSeat.navigationPoint.position;
                    agent.speed = 3;

                    SeatManager.UnoccupySeat(currentSeat);
                    CurrentState = State.Moving;

                    break;
                }

                animator.SetBool("sit", false);
                sitProgress = Mathf.MoveTowards(sitProgress, 0f, Time.deltaTime * 0.5f);
                agent.transform.position = Vector3.Lerp(currentSeat.sittingPoint.position, currentSeat.navigationPoint.position, 1 - currentSeat.StandUpAnim.Evaluate(sitProgress)) + modelSittingOffset;

                if (sitProgress == 0f)
                {
                    agent.speed = 3;
                    SeatManager.UnoccupySeat(currentSeat);
                    CurrentState = State.Moving;
                }

                break;

            case State.Sitting:

                agent.transform.position = currentSeat.sittingPoint.position + Vector3.up * 0.05f;

                if (nextSeat)
                {
                    CurrentState = State.Moving;
                    break;
                }

                CurrentState = TargetState;

                break;
        }

        previousPosition = transform.position;
    }

    private void MovementAnimRotation()
    {
        if (agent.pathPending)
            return;

        Vector3 worldDeltaPosition = agent.nextPosition - previousPosition;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);

        velocity = new Vector2(dx, dy) / Time.deltaTime;

        animator.SetFloat("velx", velocity.x);
        animator.SetFloat("vely", velocity.y);
    }
}
