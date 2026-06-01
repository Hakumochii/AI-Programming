using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Jump", story: "Jump [agent] [navMesh] with force [force]", category: "Action", id: "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6")]
public partial class JumpAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> navAgent;
    [SerializeReference] public BlackboardVariable<GameObject> agentBody;  // ← GameObject instead
    [SerializeReference] public BlackboardVariable<float> force;
    
    private NavMeshAgent _navAgent;
    private Rigidbody _rb;

    private float _jumpTime;
    private float _jumpDelay = 0.2f;  // wait before checking grounded

    protected override Status OnStart()
    {
        if (agentBody.Value == null)
        {
            Debug.LogError("JumpAction: agentBody GameObject is not assigned!");
            return Status.Failure;
        }

        _rb = agentBody.Value.GetComponent<Rigidbody>();  // ← get Rigidbody from GameObject
        if (_rb == null)
        {
            Debug.LogError("JumpAction: No Rigidbody found on agentBody!");
            return Status.Failure;
        }

        _navAgent = navAgent.Value;
        if (_navAgent != null) _navAgent.enabled = false;

        _jumpTime = Time.time;
        _rb.AddForce(Vector3.up * force.Value, ForceMode.VelocityChange);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_rb == null) return Status.Running;
        
        // Don't check grounded until jump delay has passed
        if (Time.time - _jumpTime < _jumpDelay) return Status.Running;
        
        if (_rb.linearVelocity.y <= 0 && IsGrounded())
        {
            if (_navAgent != null) _navAgent.enabled = true;
            return Status.Success;
        }
        return Status.Running;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(GameObject.transform.position, Vector3.down, 0.1f);
    }

    protected override void OnEnd()
    {
        if (_navAgent != null) _navAgent.enabled = true;
    }
}