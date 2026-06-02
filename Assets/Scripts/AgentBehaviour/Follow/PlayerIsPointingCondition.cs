using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Player is pointing", story: "player is pointing", category: "Input", id: "1837d441afeea792fd16b4ede95e8048")]
public partial class PlayerIsPointingCondition : Condition
{
    public BlackboardVariable<bool> IsPointing; 

    public override bool IsTrue() => IsPointing.Value;
}
