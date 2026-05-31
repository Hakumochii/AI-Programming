using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Idle", story: "Stand still till further instructions using [NavAgent]", category: "Action", id: "833164130bd17d26a9a4e6bcedcd7081")]
public partial class IdleAction : Action
{
    [SerializeReference] public BlackboardVariable<UnityEngine.AI.NavMeshAgent> navAgent;
    private UnityEngine.AI.NavMeshAgent _navAgent;

    protected override Status OnStart()
    {
        _navAgent = navAgent.Value;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _navAgent.enabled   = true;
        _navAgent.isStopped = true;
        _navAgent.ResetPath();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

