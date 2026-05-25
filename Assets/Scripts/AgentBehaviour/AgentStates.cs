using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AgentStates : MonoBehaviour
{
    public enum Task { Idle, FollowingTask, IndependentTask}
    public Task task = Task.Idle;

    public enum FollowingTask { GoingTowardsPlayer, Waiting }
    public FollowingTask followingTask;

    public enum IndependentTask { Spinning, Jumping }
    public IndependentTask independentTask;


    private Follow _follow;

    void Start()
    {
        _follow = FindFirstObjectByType<Follow>();
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
                FollowPlayer();
                break;
            case Task.IndependentTask:
                DoTask();
                //task check
                break;
        }
    }

    private void FollowPlayer()
    {
        switch (followingTask)
        {
            case FollowingTask.Waiting:
                _follow.Wait();
                break;
            case FollowingTask.GoingTowardsPlayer:
                _follow.GoTowardsPlayer();
                break;
        }
    }

    private void DoTask()
    {
        switch (independentTask)
        {
            case IndependentTask.Spinning:
                //Wait();
                break;
            case IndependentTask.Jumping:
                //GoTowardsPlayer();
                break;
        }
    }





}
