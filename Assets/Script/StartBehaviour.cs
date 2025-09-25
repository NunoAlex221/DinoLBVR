using UnityEngine;
using XRoam.Core.StateMachine;
using XRoam.Services.StateMachine;

public class StartBehaviour : MonoBehaviour
{
    [SerializeField] private State _state;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StateMachineService.Instance.ChangeState(_state.Id, false);

    }
}

    // Update is called once per frame