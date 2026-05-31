using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check collision name", story: "Check if collision is [targetName]", category: "Action", id: "cb9889a478a462bc6e267fddc6b50146")]
public partial class CheckCollisionNameAction : Action
{
    public BlackboardVariable<EventChannelBase> collisionEvent;
    public BlackboardVariable<string> targetName;

    private bool _eventReceived;
    private bool _isMatch;
    private System.Action<BlackboardVariable[]> _handler;

    protected override Status OnStart()
    {
        if (collisionEvent.Value == null) return Status.Failure;

        _eventReceived = false;
        _isMatch = false;

        _handler = (args) =>
        {
            if (args.Length > 0 && args[0] is BlackboardVariable<GameObject> goVar)
            {
                _isMatch = (goVar.Value.name == targetName.Value);
                _eventReceived = true;
            }
        };

        collisionEvent.Value.RegisterListener(_handler);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // Once we receive the collision data, we decide to succeed or fail
        if (_eventReceived)
        {
            return _isMatch ? Status.Success : Status.Failure;
        }

        // Wait for the BehaviorGraphCollisionEvents to dispatch the event
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (collisionEvent.Value != null)
        {
            collisionEvent.Value.UnregisterListener(_handler);
        }
    }
}

