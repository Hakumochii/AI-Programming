using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NavMeshDestination", story: "Agent navigates to [player]", category: "Action/Navigation", id: "f4dff6c6aa9f108746a34293ca8ba6f5")]
public partial class NavMeshDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

