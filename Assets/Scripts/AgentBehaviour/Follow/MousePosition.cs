using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Behavior;

public class MousePosition : MonoBehaviour
{
    public Transform groundMarker;

    [HideInInspector] public BehaviorGraphAgent agent;
    [SerializeField] private LayerMask groundLayer;

    private void Update()
    {
        if (Mouse.current == null) return;
        
        Vector2 pointerPosition = Mouse.current.position.ReadValue();
        HitGroundWithPointer(pointerPosition);
        
        agent.SetVariableValue("PointerPosition", groundMarker);
    }

    private void HitGroundWithPointer(Vector2 pointerPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            groundMarker.position = hit.point; 
        }
    }
}
