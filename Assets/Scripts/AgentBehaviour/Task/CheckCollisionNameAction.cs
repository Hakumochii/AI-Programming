using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check collision name", story: "Check if collision is [targetName]", category: "Action", id: "cb9889a478a462bc6e267fddc6b50146")]
public partial class CheckCollisionNameAction : Action
{
    public BlackboardVariable<string> targetName;

    private bool _isMatch;
    private bool _collisionHappened;
    private BehaviorGraphCollisionEvents _collisionEvents;

    protected override Status OnStart()
    {
        _collisionEvents = GameObject.GetComponent<BehaviorGraphCollisionEvents>();
        if (_collisionEvents == null) return Status.Failure;
        _isMatch = false;
        _collisionHappened = false;
        _collisionEvents.OnTriggerEnterEvent += OnTriggerEnter;
        _collisionEvents.OnTriggerExitEvent += OnTriggerExit;   
        return Status.Running;
    }

    private void OnTriggerExit(GameObject other)
    {
        if (other.name == targetName.Value)
            _isMatch = false;  
    }

    private void OnTriggerEnter(GameObject other)
    {
        _collisionHappened = true;
        if (other.name == targetName.Value)
            _isMatch = true;
    }

    protected override Status OnUpdate()
    {
        if (!_collisionHappened) return Status.Running;  
        if (_isMatch) return Status.Success;             
        return Status.Failure;                           
    }  

    protected override void OnEnd()
    {
        _isMatch = false; 
        if (_collisionEvents != null)
            _collisionEvents.OnTriggerEnterEvent -= OnTriggerEnter;
    }
}