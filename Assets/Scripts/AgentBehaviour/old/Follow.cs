/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Follow : MonoBehaviour
{
    // ─── Refs ─────────────────────────────────────────────────────────────────
    private AgentStates _state;

    [HideInInspector] public NavMeshAgent _navAgent;
    [HideInInspector] public Rigidbody rb;
    private GameObject player;

    [HideInInspector] public static event Action<int> OnAgentFollowStateChanged;
    private bool _isFollowingPlayer = false;

    // ─── Flocking parameters ──────────────────────────────────────────────────
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

    // ─── Shared registry ──────────────────────────────────────────────────────
    private static readonly List<Follow> _allAgents = new List<Follow>();

    private Vector3  _noiseOffset;
    private Coroutine _noiseCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        // GetComponent — finds the component on THIS GameObject only
        _state    = GetComponent<AgentStates>();
        _navAgent = GetComponent<NavMeshAgent>();
        rb        = GetComponent<Rigidbody>();
        player    = GameObject.FindGameObjectWithTag("Player");

        rb.isKinematic = true;
        _allAgents.Add(this);
        _noiseCoroutine = StartCoroutine(UpdateNoise());
    }

    void OnDestroy()
    {
        _allAgents.Remove(this);
        if (_noiseCoroutine != null) StopCoroutine(_noiseCoroutine);
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

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        bool wasFollowing = _isFollowingPlayer;
        _isFollowingPlayer = (_state.task == AgentStates.Task.FollowingTask);

        if (_isFollowingPlayer != wasFollowing)
            OnAgentFollowStateChanged?.Invoke(_isFollowingPlayer ? 1 : -1);

        // Spread stopped agents apart without re-triggering movement
        if (_isFollowingPlayer && _state.followingTask == AgentStates.FollowingTask.Waiting)
            ApplyWaitingSeparation();
    }

    // ─── Called by AgentStates ────────────────────────────────────────────────

    public void GoTowardsPlayer()
    {
        _navAgent.enabled   = true;
        rb.isKinematic      = true;
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

    public void Wait()
    {
        _navAgent.enabled   = true;
        rb.isKinematic      = true;
        _navAgent.isStopped = true;
        _navAgent.ResetPath();
    }

    // ─── Separation while stationary ─────────────────────────────────────────

    // Gently warp stopped agents apart so they don't stack.
    // The nudge is tiny so it never pushes an agent outside PlayerRange.
    private void ApplyWaitingSeparation()
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
            sep -= (toOther / dist) * strength;
        }

        if (sep == Vector3.zero) return;

        Vector3 nudge  = sep.normalized * (separationForce * 0.05f * Time.deltaTime);
        Vector3 target = transform.position + nudge;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            _navAgent.Warp(hit.position);
    }

    // ─── Separation offset while moving ──────────────────────────────────────

    // Only separation (not cohesion/alignment) so the destination stays
    // near the player and agents don't overshoot or orbit
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

    // ─── Triggers ────────────────────────────────────────────────────────────
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

}*/