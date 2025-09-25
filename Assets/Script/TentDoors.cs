using UnityEngine;
using UnityEngine.Playables;

public class TentDoors : MonoBehaviour
{
    public PlayableDirector door1Director;
    public PlayableDirector door2Director;
    public float delay = 15f;

    [Header("Trigger")]
    [Tooltip("Deixa vazio para aceitar qualquer colisão")]
    public string requiredTag = "Player";
    public bool disableTriggerAfter = true;

    void Start()
    {
        Invoke(nameof(PlayTimelines), delay);
    }

    void Reset()
    {
        // garante que este objeto é mesmo um trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void PlayTimelines()
    {
        if (door1Director) door1Director.Play();
        if (door2Director) door2Director.Play();
    }

    // --- Fecha (volta ao estado inicial) quando colidir ---
    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        ResetDirector(door1Director);
        ResetDirector(door2Director);

        if (disableTriggerAfter) gameObject.SetActive(false);
    }

    // Volta o Timeline para o frame 0 e aplica a pose inicial
    static void ResetDirector(PlayableDirector d)
    {
        if (!d) return;
        d.time = 0;
        d.Evaluate();  // aplica o estado do início
        d.Stop();      // opcional: liberta o controlo do Timeline
    }
}