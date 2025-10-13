using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class PicoHandGrab : MonoBehaviour
{
    XRHandSubsystem handSubsystem;
    [SerializeField] Transform handTransform;

    [Header("Settings")]
    public bool useRightHand = true;
    public float grabThreshold = 0.03f; // Distance (meters) between thumb and index for grab

    bool isGrabbing = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the active XR Hand Subsystem
        handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader
            ?.GetLoadedSubsystem<XRHandSubsystem>();

        if (handSubsystem == null)
            Debug.LogWarning("XRHandSubsystem not found! Make sure Hand Tracking is enabled in OpenXR Features.");
    }

    void Update()
    {
        if (handSubsystem == null) return;

        XRHand hand = useRightHand ? handSubsystem.rightHand : handSubsystem.leftHand;
        if (!hand.isTracked) return;

        // Get thumb and index tip joints
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (!thumbTip.TryGetPose(out Pose thumbPose) || !indexTip.TryGetPose(out Pose indexPose))
            return;

        // Measure the distance between thumb and index tip
        float distance = Vector3.Distance(thumbPose.position, indexPose.position);

        // Grab detection
        if (!isGrabbing && distance < grabThreshold)
        {
            isGrabbing = true;
            OnGrabStart();
        }
        else if (isGrabbing && distance > grabThreshold * 1.2f)
        {
            isGrabbing = false;
            OnGrabEnd();
        }
    }

    void OnGrabStart()
    {
        Debug.Log("Grab Started");
        // Example: parent nearest object to hand
        Collider[] hits = Physics.OverlapSphere(handTransform.position, 0.05f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Grabbable"))
            {
                hit.transform.SetParent(handTransform);
                break;
            }
        }
    }

    void OnGrabEnd()
    {
        Debug.Log("Grab Released");
        // TODO: Detach or drop object here
    }
}
