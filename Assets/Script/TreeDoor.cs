using UnityEngine;
using UnityEngine.Playables;

public class TriggerPlayableDirector : MonoBehaviour
{
    public PlayableDirector director;   // drag your PlayableDirector here
    public string requiredTag = "Player"; // leave empty to accept any collider
    public bool playFromStart = true;     // restart from 0 when triggered
    public bool onlyOnce = true;          // trigger just one time
    public float delaySeconds = 0f;       // optional delay before play

    bool done;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true; // make this collider a trigger
    }

    void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && done) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
        if (!director) return;

        done = true;
        if (delaySeconds > 0f) Invoke(nameof(PlayNow), delaySeconds);
        else PlayNow();
    }

    void PlayNow()
    {
        if (!director) return;
        if (playFromStart)
        {
            director.time = 0;
            director.Evaluate();   // apply first frame immediately (optional)
        }
        director.Play();
    }
}