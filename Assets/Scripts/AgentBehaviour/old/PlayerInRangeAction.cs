using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Player in range", story: "[player] is in range", category: "Action/Conditional", id: "56cbd7a60cd5a80e5be91f2f30ce3161")]
public partial class PlayerInRangeAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> player;
    private BehaviorGraphAgent _agent;
    private Vector3 _player;
    private float arrivalDistance = 2f;

    protected override Status OnStart()
    {
        _agent = GameObject.GetComponent<BehaviorGraphAgent>();
        _player = player.Value.position;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float dist = Vector3.Distance(_agent.transform.position, _player);
        if (dist < arrivalDistance)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

