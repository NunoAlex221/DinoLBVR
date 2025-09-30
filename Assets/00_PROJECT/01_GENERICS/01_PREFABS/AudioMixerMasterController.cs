using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerMasterController : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _exposedParameter;
    [SerializeField] private float _fadeDuration = 1.0f;

    public void FadeToVolume(float volume)
    {
        StartCoroutine(FadeMixerGroup(volume));
    }



    private IEnumerator FadeMixerGroup(float targetVolume)
    {
        float currentTime = 0;
        _audioMixer.GetFloat(_exposedParameter, out float currentVolume);
        currentVolume = Mathf.Pow(10, currentVolume / 20);

        float targetLinear = Mathf.Pow(10, targetVolume / 20);

        while (currentTime < _fadeDuration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(currentVolume, targetLinear, currentTime / _fadeDuration);
            _audioMixer.SetFloat(_exposedParameter, Mathf.Log10(newVolume) * 20);
            yield return null;
        }

        _audioMixer.SetFloat(_exposedParameter, targetVolume);
    }


}
