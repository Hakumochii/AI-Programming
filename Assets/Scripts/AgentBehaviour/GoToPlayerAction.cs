using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Go to player", story: "go to [player] using [navmesh]", category: "Action", id: "c52435725df37be1c8af6ddef6c6b9f4")]
public partial class GoToPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> player;
    [SerializeReference] public BlackboardVariable<NavMeshAgent> _navAgent;

    protected override Status OnStart()
    {
        player    = GameObject.FindWithTag("Player");
        _state    = GetComponent<AgentStates>();
        _navAgent = GetComponent<NavMeshAgent>();
        _allAgents.Add(this);
        _noiseCoroutine = StartCoroutine(UpdateNoise());
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GoTowardsPlayer();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    public void GoTowardsPlayer()
    {
        _navAgent.enabled   = true;
        _navAgent.isStopped = false;

        // Only use separation while moving — no cohesion/alignment offset
        // so the destination stays close to the player and doesn't drift
        Vector3 sep = ComputeSeparationOnly();
        if (sep.magnitude > maxFlockingOffset)
            sep = sep.normalized * maxFlockingOffset;

        Vector3 destination = player.transform.position + sep + _noiseOffset;
        destination.y = player.transform.position.y;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, maxFlockingOffset + 1f, NavMesh.AllAreas))
            _navAgent.SetDestination(hit.position);
        else
            _navAgent.SetDestination(player.transform.position);
    }

    IEnumerator UpdateNoise()
    {
        float interval = 1.0f / Mathf.Max(noiseFrequency, 0.01f);
        while (true)
        {
            _noiseOffset = new Vector3(
                (UnityEngine.Random.value * 2f - 1f) * noiseForce,
                0f,
                (UnityEngine.Random.value * 2f - 1f) * noiseForce
            );
            yield return new WaitForSeconds(interval);
        }
    }

    

    private Vector3 ComputeSeparationOnly()
    {
        Vector3 sep   = Vector3.zero;
        Vector3 myPos = transform.position;

        foreach (Follow other in _allAgents)
        {
            if (other == this || other == null) continue;
            if (other._state.task != AgentStates.Task.FollowingTask) continue;

            Vector3 toOther = other.transform.position - myPos;
            toOther.y       = 0f;
            float dist      = toOther.magnitude;

            if (dist < 0.001f || dist >= separationRadius) continue;

            float strength = 1f - (dist / separationRadius);
            sep -= (toOther / dist) * strength * separationForce;
        }

        sep.y = 0f;
        return sep;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _state.task          = AgentStates.Task.FollowingTask;
            _state.followingTask = AgentStates.FollowingTask.Waiting;
        }

        if (collision.gameObject.CompareTag("PlayerRange") && _state.task == AgentStates.Task.FollowingTask)
        {
            _state.followingTask = AgentStates.FollowingTask.Waiting;
        }

        
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("PlayerRange") && _state.task == AgentStates.Task.FollowingTask)
        {
            _state.followingTask = AgentStates.FollowingTask.GoingTowardsPlayer;
        }
    }
}

