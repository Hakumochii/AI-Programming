using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Behavior;

public class PlayerInteraction : MonoBehaviour
{
    //input
    private PlayerInput _playerInput;

    //reference to cursors
    [SerializeField] private GameObject cursorCircleSmallPrefab;
    [SerializeField] private GameObject cursorCircleBigPrefab;
    [SerializeField] private GameObject arrowCursorPrefab;
    private GameObject cursorCircleSmall;
    private GameObject arrowCursor;
    private GameObject cursorCircleBig;
    private GameObject currentCursorCircleBig; 

    //reference to player and ground, task and agent layers
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask taskLayer;
    [SerializeField] private LayerMask agentLayer;
    public GameObject player;

    //vaiables used for states or calculations
    private bool callingAgents = false; 
    private bool directing = false;
    private Quaternion cursorRotation = Quaternion.Euler(90f, 0f, 0f);
    private float cursorOffsetFromGround = 0.5f;
    public float sphereRadius = 2f;

    //corutines
    private Coroutine callAgentsCoroutine;

    //animation
    public Animator _animator;
    
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined; 

        cursorCircleSmall = Instantiate(cursorCircleSmallPrefab, Vector3.zero, cursorRotation);
        cursorCircleBig = Instantiate(cursorCircleBigPrefab, Vector3.zero, cursorRotation);
        arrowCursor = Instantiate(arrowCursorPrefab, Vector3.zero, cursorRotation);
        
        cursorCircleSmall.SetActive(false);
        cursorCircleBig.SetActive(false);
        arrowCursor.SetActive(false);
    }

    public void OnDirect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            directing = true;
            _animator.SetBool("Pointing", true);
        }
        else if (context.canceled)
        {
            directing = false;
            _animator.SetBool("Pointing", false);
        }

        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        foreach (GameObject agent in agents)
        {
            var states = agent.GetComponent<AgentStates>();
            if (states == null) continue;

            if (states.task == AgentStates.Task.FollowingTask)
            {
                var behaviorAgent = states.followGraph.GetComponent<BehaviorGraphAgent>();
                if (behaviorAgent != null)
                {
                    behaviorAgent.BlackboardReference.SetVariableValue("IsPointing", directing);
                    behaviorAgent.Restart();
                }
            }
            else
            {
                var behaviorAgent = states.followGraph.GetComponent<BehaviorGraphAgent>();
                if (behaviorAgent != null)
                    behaviorAgent.BlackboardReference.SetVariableValue("IsPointing", directing);
            }
        }
    }

    public void OnCall(InputAction.CallbackContext context)
    {
        if (context.performed)
        {    
            _animator.SetBool("Yelling", true);
            if (callAgentsCoroutine == null)
            {
                callingAgents = true;
                callAgentsCoroutine = StartCoroutine(CallAgentsContinuously());
            }
        }
        else if (context.canceled)
        {
            _animator.SetBool("Yelling", false);
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

        if (Physics.SphereCast(ray, sphereRadius, out hit, Mathf.Infinity, groundLayer))
        {
            ProjectCursor(hit);
        }
        else
        {
            cursorCircleSmall.SetActive(false);
            cursorCircleBig.SetActive(false);
            arrowCursor.SetActive(false);
        }
    }

    private void ProjectCursor(RaycastHit hit)
    {
        Vector3 pos = new Vector3(hit.point.x, hit.point.y + cursorOffsetFromGround, hit.point.z);

        if (callingAgents)
        {
            cursorCircleSmall.SetActive(false);
            arrowCursor.SetActive(false);
            cursorCircleBig.SetActive(true);
            cursorCircleBig.transform.position = pos;
        }
        else if (directing)
        {
            cursorCircleBig.SetActive(false);
            cursorCircleSmall.SetActive(false);
            arrowCursor.SetActive(true);
            arrowCursor.transform.position = pos;
        }
        else
        {
            cursorCircleBig.SetActive(false);
            arrowCursor.SetActive(false);
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
