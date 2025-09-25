using UnityEngine;
using System.Collections;
using XRoam.Core.States;

public class PortalSequence : MonoBehaviour
{
    public PortalSphere portal;
    public Transform target;
    public StateChanger stateChanger;
    public float delayBeforeChange = 2f; // tempo para ver o FX

    public void Activate(StateChanger Changer)
    {
        stateChanger = Changer;
        target = GameObject.FindGameObjectWithTag("Player").transform;
        if (portal) portal.PlayAt(target);
        StartCoroutine(Co());
    }

    IEnumerator Co()
    {
        yield return new WaitForSeconds(delayBeforeChange);
        if (stateChanger) stateChanger.ChangeState();
    }
}