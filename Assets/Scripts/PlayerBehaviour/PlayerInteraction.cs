using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    //input
    private PlayerInput _playerInput;

    //reference to cursors
    [SerializeField] private GameObject cursorCircleSmallPrefab;
    [SerializeField] private GameObject cursorCircleBigPrefab;
    private GameObject cursorCircleSmall;
    private GameObject cursorCircleBig;
    private GameObject currentCursorCircleBig; 

    //reference to player and ground, task and agent layers
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask taskLayer;
    [SerializeField] private LayerMask agentLayer;
    public GameObject player;

    //vaiables used for states or calculations
    private bool callingAgents = false; 
    private Quaternion cursorRotation = Quaternion.Euler(90f, 0f, 0f);
    private float cursorOffsetFromGround = 0.5f;
    private Vector2 _lastPointerPosition;

    //corutines
    private Coroutine callAgentsCoroutine;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined; // keeps it inside the game window

        cursorCircleSmall = Instantiate(cursorCircleSmallPrefab, Vector3.zero, cursorRotation);
        cursorCircleBig = Instantiate(cursorCircleBigPrefab, Vector3.zero, cursorRotation);
        
        cursorCircleSmall.SetActive(false);
        cursorCircleBig.SetActive(false);
    }

    public void OnCall(InputAction.CallbackContext context)
    {
        if (context.performed && callAgentsCoroutine == null)
        {
            callingAgents = true;
            callAgentsCoroutine = StartCoroutine(CallAgentsContinuously());
        }
        else if (context.canceled)
        {
            callingAgents = false;
            if (callAgentsCoroutine != null)
            {
                StopCoroutine(callAgentsCoroutine);
                callAgentsCoroutine = null;
            }
        }
    }

    public void OnDissmis(InputAction.CallbackContext context)
    {
        DismissAgents();
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        
        Vector2 pointerPosition = Mouse.current.position.ReadValue();
        HitGroundWithPointer(pointerPosition);
    }

    private void HitGroundWithPointer(Vector2 pointerPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            ProjectCursor(hit);
        }
        else
        {
            // Raycast missed — hide cursors so you can see when this happens
            cursorCircleSmall.SetActive(false);
            cursorCircleBig.SetActive(false);
        }
    }

    private void ProjectCursor(RaycastHit hit)
    {
        Vector3 pos = new Vector3(hit.point.x, hit.point.y + cursorOffsetFromGround, hit.point.z);

        if (callingAgents)
        {
            cursorCircleSmall.SetActive(false);
            cursorCircleBig.SetActive(true);
            cursorCircleBig.transform.position = pos;
        }
        else
        {
            cursorCircleBig.SetActive(false);
            cursorCircleSmall.SetActive(true);
            cursorCircleSmall.transform.position = pos;
        }
    }

    private IEnumerator CallAgentsContinuously()
    {
        while (callingAgents)
        {
            Vector2 pointerPosition = Mouse.current.position.ReadValue();
            CallAgents(pointerPosition);
            yield return null;
        }
        callAgentsCoroutine = null;
    }

    private void CallAgents(Vector2 pointerPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit hit;
        float rayRadius = 3f;

        if (Physics.SphereCast(ray, rayRadius, out hit, Mathf.Infinity, agentLayer))
        {
            if (hit.collider != null && hit.collider.CompareTag("Agent"))
            {
                GameObject agent = hit.collider.gameObject;
                AgentStates _state = agent.GetComponent<AgentStates>();
                if (_state != null)
                {
                    if (_state.task != AgentStates.Task.FollowingTask)
                    {
                        _state.task = AgentStates.Task.FollowingTask;
                        _state.followingTask = AgentStates.FollowingTask.GoingTowardsPlayer; 
                    }
                    
                }
            }
        }
    }

    private void DismissAgents()
    {
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Agent");
        
        foreach (GameObject agent in allAgents)
        {
            
            AgentStates _state = agent.GetComponent<AgentStates>();
            if (_state != null && _state.task == AgentStates.Task.FollowingTask)
            {
                _state.task = AgentStates.Task.Idle;
            }
        }
    }
}
