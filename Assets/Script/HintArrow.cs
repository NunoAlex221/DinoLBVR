using UnityEngine;
using XRoam.Core.StateMachine;
using XRoam.Services.StateMachine;
public class EnableAfterDelay : MonoBehaviour
{
    public float delaySeconds;

    public Transform target;
    [SerializeField] private State _state;
    [SerializeField] private PortalSphere effect;
    void Start()
    {
        Invoke(nameof(ChangeState), delaySeconds);
        effect = GameObject.FindGameObjectWithTag("Portal").GetComponent<PortalSphere>();
    }

    public void ChangeState()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        if (effect) effect.PlayAt(target);

        StateMachineService.Instance.ChangeState(_state.Id, false);
    }
}