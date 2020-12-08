using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class MovementIndicator : MonoBehaviour
{
    public Transform indicator;
    public LineRenderer line;
    public NavMeshAgent agent;
    public NavAgentAnimation anim;

    public Vector3 offset;

    void Start()
    {
        line.positionCount = 0;
    }

    void Update()
    {
        if (agent.remainingDistance < 0.1f || 
            anim.CurrentState == NavAgentAnimation.State.Sitting || 
            anim.CurrentState == NavAgentAnimation.State.MoveToSit)
        {
            line.positionCount = 0;
            indicator.gameObject.SetActive(false);

            return;
        }

        line.positionCount = agent.path.corners.Length;

        for (int i = 0; i < agent.path.corners.Length; ++i)
        {
            line.SetPosition(agent.path.corners.Length - 1 - i, agent.path.corners[i] + offset);
        }

        indicator.gameObject.SetActive(true);
        indicator.position = agent.destination + Vector3.up * 0.01f + offset;    
    }
}
