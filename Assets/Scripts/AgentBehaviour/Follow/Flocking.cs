using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Flocking", story: "Go to [target] while flocking, using [NavAgent]", category: "Action", id: "c52435725df37be1c8af6ddef6c6b9f4")]
public partial class Flocking : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> navAgent;
    [SerializeReference] public BlackboardVariable<Transform> target;
    private Vector3 _target;
    private BehaviorGraphAgent _agent;
    private NavMeshAgent _navAgent;

    [Header("Flocking - Separation")]
    public float separationRadius = 3.0f;
    public float separationForce  = 5.0f;

    [Header("Flocking - Cohesion")]
    public float cohesionRadius = 8.0f;
    public float cohesionForce  = 1.0f;

    [Header("Flocking - Alignment")]
    public float alignmentForce = 1.0f;

    [Header("Flocking - Noise")]
    public float noiseForce     = 0.5f;
    public float noiseFrequency = 0.5f;

    [Header("Destination")]
    [Tooltip("Max world-unit shift the flocking offset can apply to the NavMesh destination.")]
    public float maxFlockingOffset = 2.5f;

    private static readonly List<BehaviorGraphAgent> _allAgents = new List<BehaviorGraphAgent>();

    private Vector3   _noiseOffset;
    private Coroutine _noiseCoroutine;
    private float     arrivalDistance = 4f;


    protected override Status OnStart()
    {
        _agent    = GameObject.GetComponent<BehaviorGraphAgent>();
        _navAgent = navAgent.Value;
        _noiseCoroutine = _agent.StartCoroutine(UpdateNoise());
        _allAgents.Add(_agent);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _target = target.Value.position;
        GoTowardsTarget();

        float dist = Vector3.Distance(_agent.transform.position, _target);
        if (dist < arrivalDistance)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_noiseCoroutine != null)
        {
            _agent.StopCoroutine(_noiseCoroutine);
            _noiseCoroutine = null;
        }

        _allAgents.Remove(_agent);
    }

    public void GoTowardsTarget()
    {
        _navAgent.enabled   = true;
        _navAgent.isStopped = false;

        Vector3 sep       = ComputeSeparation();
        Vector3 cohesion  = ComputeCohesion();
        Vector3 alignment = ComputeAlignment();

        // Clamp each force independently so none dominates
        if (sep.magnitude       > maxFlockingOffset) sep       = sep.normalized       * maxFlockingOffset;
        if (cohesion.magnitude  > maxFlockingOffset) cohesion  = cohesion.normalized  * maxFlockingOffset;
        if (alignment.magnitude > maxFlockingOffset) alignment = alignment.normalized * maxFlockingOffset;

        Vector3 destination = _target + sep + cohesion + alignment + _noiseOffset;
        destination.y = _target.y;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, maxFlockingOffset + 1f, NavMesh.AllAreas))
            _navAgent.SetDestination(hit.position);
        else
            _navAgent.SetDestination(_target);
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

    // Steer away from nearby agents
    private Vector3 ComputeSeparation()
    {
        Vector3 sep   = Vector3.zero;
        Vector3 myPos = _agent.transform.position;

        foreach (BehaviorGraphAgent other in _allAgents)
        {
            if (other == _agent || other == null) continue;

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

    // Steer toward the average position of all other agents
    private Vector3 ComputeCohesion()
    {
        if (_allAgents.Count <= 1) return Vector3.zero;

        Vector3 avgPos = Vector3.zero;
        int     count  = 0;

        foreach (BehaviorGraphAgent other in _allAgents)
        {
            if (other == _agent || other == null) continue;
            avgPos += other.transform.position;
            count++;
        }

        if (count == 0) return Vector3.zero;

        avgPos /= count;

        Vector3 toCenter = (avgPos - _agent.transform.position);
        toCenter.y = 0f;
        return toCenter.normalized * cohesionForce;
    }

    // Steer to match the average velocity of all other agents
    private Vector3 ComputeAlignment()
    {
        if (_allAgents.Count <= 1) return Vector3.zero;

        Vector3 avgVelocity = Vector3.zero;
        int     count       = 0;

        foreach (BehaviorGraphAgent other in _allAgents)
        {
            if (other == _agent || other == null) continue;

            NavMeshAgent otherNav = other.GetComponent<NavMeshAgent>();
            if (otherNav == null) continue;

            avgVelocity += otherNav.velocity;
            count++;
        }

        if (count == 0) return Vector3.zero;

        avgVelocity /= count;
        avgVelocity.y = 0f;
        return avgVelocity.normalized * alignmentForce;
    }
}