using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Is Pointing", story: "player is pointing", category: "Action/Conditional", id: "3ac11d0226dc6066ddab7cb190445adb")]
public partial class IsPointingAction : Action
{
    private Action<BlackboardVariable[]> _handler;
    [SerializeReference] public BlackboardVariable<EventChannelBase> pointEvent;
    private bool _isPointing;

    protected override Status OnStart()
    {
        _handler = (BlackboardVariable[] values) =>
        {
            if (values.Length > 0 && values[0] is BlackboardVariable<bool> boolVar)
                _isPointing = boolVar.Value;
        };
        pointEvent.Value.RegisterListener(_handler);
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
        pointEvent.Value.UnregisterListener(_handler);
    }

    public bool IsTrue() => _isPointing;
}

