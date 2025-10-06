using UnityEngine;


[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    public CanvasGroup iconGroup;    // CanvasGroup on the icon
    public float fadeSpeed = 5f;     // Speed of fade
    public float maxDistance = 5f;   // How close the player must be

    public float gazeRadius = 0.3f; // <-- make this bigger to make detection easier

    Transform playerHead;
    bool shouldShow = false;

    void Start()
    {
        if (iconGroup != null)
            iconGroup.alpha = 0f;
    }

    void Update()
    {
        if (playerHead == null)
        {
            if (Camera.main != null)
                playerHead = Camera.main.transform;
            else
                return; // just skip this frame, no errors
        }

        // Cast a ray forward from the VR camera
        Ray ray = new Ray(playerHead.position, playerHead.forward);
        if (Physics.SphereCast(ray, gazeRadius, out RaycastHit hit, maxDistance))
        {
            // Check if THIS object was hit
            shouldShow = hit.collider.gameObject == gameObject;
        }
        else
        {
            shouldShow = false;
        }

        // Smooth fade
        float targetAlpha = shouldShow ? 1f : 0f;
        iconGroup.alpha = Mathf.Lerp(iconGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        Vector3 lookDir = playerHead.position - transform.position;

        // Zero out vertical component so it only rotates around Y
        lookDir.y = 0f;

        // If the player is directly above/below, ignore to avoid NaN
        if (lookDir.sqrMagnitude > 0.001f)
            iconGroup.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}

