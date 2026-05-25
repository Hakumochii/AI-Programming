using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Follow : MonoBehaviour
{
    private AgentStates _state;

    [HideInInspector] public NavMeshAgent _navAgent;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] private GameObject player;

    [HideInInspector] public static event Action<int> OnAgentFollowStateChanged; 
    private bool _isFollowingPlayer = false;

    void Start()
    {
        _state = FindFirstObjectByType<AgentStates>();
        _navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");

        //initailly disable physics
        rb.isKinematic = true;
    }

    void Update()
    {
        //checks if task == Task.FollowingTask is true or not if its true a pikmin wil be added to the following variable in game manager
        bool wasFollowing = _isFollowingPlayer;
        _isFollowingPlayer = (_state.task == AgentStates.Task.FollowingTask);

        if (_isFollowingPlayer != wasFollowing)
        {
            // Notify state change (+1 if following, -1 if other)
            OnAgentFollowStateChanged?.Invoke(_isFollowingPlayer ? 1 : -1);
        }
    }

    public void GoTowardsPlayer()
    {
        _navAgent.enabled = true;
        rb.isKinematic = true;
        _navAgent.isStopped = false;
        _navAgent.SetDestination(player.transform.position);
    }

    public void Wait()
    {
        _navAgent.enabled = true;
        rb.isKinematic = true;
        _navAgent.isStopped = true;
        _navAgent.ResetPath();
    }

    private void OnTriggerEnter(Collider collision)
    {
        //if in contact with player the agent is set to follow the player 
        //but its state is set to wait because the agent would be too close to the player
        if (collision.gameObject.CompareTag("Player"))
        {
            _state.task = AgentStates.Task.FollowingTask;  
            _state.followingTask = AgentStates.FollowingTask.Waiting;        
        }

        //if the agent is too close to the player it is told to wait
        if (collision.gameObject.CompareTag("PlayerRange") && _state.task == AgentStates.Task.FollowingTask)
        {
            _state.followingTask = AgentStates.FollowingTask.Waiting;
        }  

    }

    private void OnTriggerExit(Collider collision)
    {
         //if the pikmin is told to follow the player but the player gets too far away the pikmin will start going towards the player
        if (collision.gameObject.CompareTag("PlayerRange") && _state.task == AgentStates.Task.FollowingTask)
        {
            _state.followingTask = AgentStates.FollowingTask.GoingTowardsPlayer;
        }
    } 
}
