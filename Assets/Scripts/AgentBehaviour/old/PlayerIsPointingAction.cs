using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Player is pointing", story: "player is directing the agent", category: "Action", id: "d82aa8f1e32a1ba1274e4ef1edfd4881")]
public partial class PlayerIsPointingAction : Action
{
    public BlackboardVariable<bool> isPointing;
    public BlackboardVariable<EventChannelBase> pointEvent;

    private Action<BlackboardVariable[]> _handler;

    protected override Status OnStart()
    {
        _handler = OnEventReceived;
        pointEvent.Value.RegisterListener(_handler);
        return Status.Running;
    }

    private void OnEventReceived(BlackboardVariable[] args)
    {
        if (args.Length == 0) return;

        if (args[0] is BlackboardVariable<bool> value)
        {
            isPointing.Value = value.Value;
        }
    }

    protected override Status OnUpdate()
    {
        return Status.Running; // just stays active
    }

    protected override void OnEnd()
    {
        pointEvent.Value.UnregisterListener(_handler);
    }
}


