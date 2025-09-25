using UnityEngine;

public class EnableAfterDelay : MonoBehaviour
{
    public GameObject target;   // drag the Path parent here
    public float delaySeconds = 10f;

    void Start() => Invoke(nameof(Show), delaySeconds);
    void Show() { if (target) target.SetActive(true); }
}