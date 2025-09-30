dusing UnityEngine;
using System.Collections;

public class PortalSphere : MonoBehaviour
{
    public ParticleSystem ps;
    public float growTime = 0.4f;
    public float lifeTime = 1.2f;
    public bool parentToTarget = true;   // prende o efeito ao jogador durante o play
    

    // chama isto no State Trigger
    public void PlayAt(Transform target)
    {
        if (!ps) ps = GetComponent<ParticleSystem>();
        if (!ps) return;

        // SEPARADO DO "PORTAL": ativa o GameObject que tem o ParticleSystem
        ps.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(CoPlay(target));
    }

    IEnumerator CoPlay(Transform target)
    {
        var yourParticleEmission = ps.emission;
        Transform t = ps.transform;      // usa o GO do PS
        // posiciona e orienta para o forward da câmara (se existir)
        if (target) transform.position = target.position;
        var cam = Camera.main;
        if (cam) t.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);

        t.localScale = Vector3.one * 0.1f;

        ps.Clear(true);
        ps.Simulate(0f, true, true);
        ps.Play(true);

        float tLerp = 0f;
        while (tLerp < 1f)
        {
            tLerp += Time.deltaTime / Mathf.Max(0.0001f, growTime);
            float k = Mathf.SmoothStep(0, 1, tLerp);
            t.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 3f, k);
            yield return null;
        }

        while (tLerp < 5f)
        {
            tLerp += Time.deltaTime / Mathf.Max(0.0001f, growTime);
            yourParticleEmission.rateOverTime = Mathf.Lerp(0, 500, 5 - tLerp);
            yield return null;
        }

        yield return new WaitForSeconds(lifeTime);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (parentToTarget) t.SetParent(null);
        ps.gameObject.SetActive(false);
    }
}