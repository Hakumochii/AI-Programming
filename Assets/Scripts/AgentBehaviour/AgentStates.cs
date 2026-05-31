using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Unity.Behavior;

public class AgentStates : MonoBehaviour
{
    public enum Task { Idle, FollowingTask, IndependentTask}
    public Task task = Task.Idle;

    public GameObject followGraph;
    public GameObject idleGraph;
    public GameObject taskGraph;

    private List<GameObject> _graphs = new List<GameObject>();

    void Awake()
    {
        _graphs.Add(followGraph);
        _graphs.Add(idleGraph);
        _graphs.Add(taskGraph);
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
                SetGraphActive(followGraph);
                break;
            case Task.IndependentTask:
                //task check
                break;
        }
    }

    private void SetGraphActive(GameObject graph)
    {
        foreach (var g in _graphs)
            g.SetActive(g == graph);
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("trigger collision detected");
        if (collision.gameObject.CompareTag("Player"))
        {
            task = Task.FollowingTask;
            Debug.Log("change to following");
        }
  
    }


}
