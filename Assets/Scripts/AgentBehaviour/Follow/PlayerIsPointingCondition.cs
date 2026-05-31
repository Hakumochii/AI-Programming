using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Player is pointing", story: "player is pointig", category: "Input", id: "1837d441afeea792fd16b4ede95e8048")]
public partial class PlayerIsPointingCondition : Condition
{
    public BlackboardVariable<EventChannelBase> pointEvent;
    private bool _isPointing;
    private Action<BlackboardVariable[]> _handler;

    public override bool IsTrue() => _isPointing; 

    public override void OnStart()
    {
        if (pointEvent.Value == null) return;
        _handler = OnEventReceived;
        pointEvent.Value.RegisterListener(_handler);
    }

    private void OnEventReceived(BlackboardVariable[] args)
    {
        if (args.Length == 0) return;
        if (args[0] is BlackboardVariable<bool> val)
            _isPointing = val.Value;
    }

    public override void OnEnd()
    {
        if (pointEvent.Value != null)
            pointEvent.Value.UnregisterListener(_handler);
    }
}
