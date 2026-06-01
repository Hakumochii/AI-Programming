using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Unity.Behavior;

public class AgentStates : MonoBehaviour
{
    public enum Task { Idle, FollowingTask, IndependentTask}
    private Task _task = Task.Idle;
    public Task task
    {
        get => _task;
        set
        {
            if (_task == value) return;
            _task = value;
            OnTaskChanged(_task);
        }
    }

    public GameObject followGraph;
    public GameObject idleGraph;
    public GameObject taskGraph;

    private List<GameObject> _graphs = new List<GameObject>();

    public Rigidbody agentBody;  // ← drag it here in the inspector

    void Awake()
    {
        _graphs.Add(followGraph);
        _graphs.Add(idleGraph);
        _graphs.Add(taskGraph);

        foreach (var g in _graphs)
            g.SetActive(true);
    }

    IEnumerator Start()
    {
        yield return null; // wait one frame
        
        var agent = taskGraph.GetComponent<BehaviorGraphAgent>();
        agent.BlackboardReference.SetVariableValue("AgentBody", agentBody);
        
        // Verify it was set
        agent.BlackboardReference.GetVariableValue("AgentBody", out Rigidbody rb);
        
        yield return null; // wait another frame before activating
        
        SetGraphActive(idleGraph);
    }

    private void OnTaskChanged(Task newTask)
    {
        switch (newTask)
        {
            case Task.Idle:
                SetGraphActive(idleGraph);
                break;
            case Task.FollowingTask:
                SetGraphActive(followGraph);
                break;
            case Task.IndependentTask:
                SetGraphActive(taskGraph);
                break;
        }
    }

    private void SetGraphActive(GameObject graph)
    {
        foreach (var g in _graphs)
        {
            if (g == graph)
            {
                g.SetActive(true);
                // Restart the graph so all nodes reset cleanly
                g.GetComponent<BehaviorGraphAgent>().Restart();
            }
            else
            {
                g.SetActive(false);
            }
        }
}

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            task = Task.FollowingTask;
        }

        if (collision.gameObject.CompareTag("Task"))
        {
            task = Task.IndependentTask;
        }
  
    }

    

}
