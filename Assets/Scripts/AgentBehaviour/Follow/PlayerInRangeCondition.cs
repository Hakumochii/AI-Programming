using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Player in range", story: "player is in range", category: "Conditions", id: "05756edaf7bfa67e10c0f9087120f2de")]
public partial class PlayerInRangeCondition : Condition
{
    public BlackboardVariable<Transform> player;
    private BehaviorGraphAgent _agent;
    private float arrivalDistance = 2f;

    public override void OnStart()
    {
        _agent = GameObject.GetComponent<BehaviorGraphAgent>();
    }

    public override bool IsTrue()
    {
        if (_agent == null || player.Value == null) return false;
        float dist = Vector3.Distance(_agent.transform.position, player.Value.position);
        return dist < arrivalDistance;
    }

    public override void OnEnd() { }

}
