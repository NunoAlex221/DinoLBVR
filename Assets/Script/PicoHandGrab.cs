using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class PicoHandGrab : MonoBehaviour
{
    [SerializeField] GameObject parent;
    [SerializeField] Vector3 startingPosition;
    [SerializeField] Quaternion startingRotation;

    private void Start()
    {
        startingPosition = gameObject.transform.localPosition;
        startingRotation = gameObject.transform.localRotation;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Check"))
        {
            transform.SetParent(parent.transform);
            transform.localPosition = startingPosition;
            transform.localRotation = startingRotation;
        }
    }
}
