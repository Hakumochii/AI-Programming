using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Unity.Behavior;

public class AgentStates : MonoBehaviour
{
    public BehaviorGraphAgent agent; 
    public enum Task { Idle, FollowingTask, IndependentTask}
    public Task task = Task.Idle;

    public BehaviorGraph followGraph;
    public BehaviorGraph idleGraph;
    public BehaviorGraph taskGraph;

    void Start()
    {
        agent = GetComponent<BehaviorGraphAgent>();
    }


    private void Update()
    {
        //switching tasks based on differnet factors
        switch (task)
        {
            case Task.Idle:
                //no task
                break;
            case Task.FollowingTask:
                AssignGraph(followGraph);
                break;
            case Task.IndependentTask:
                //task check
                break;
        }
    }

    private void AssignGraph(BehaviorGraph graph)
    {
        agent.Graph = graph;
    }


}
