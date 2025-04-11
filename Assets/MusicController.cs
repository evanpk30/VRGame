using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SequentialAudioPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip firstSound;
    public AudioClip secondSound;

    [Header("Timing Settings")]
    [Tooltip("Delay between the end of first sound and start of second")]
    public float delayBetweenSounds = 1f;

    [Header("Fade Settings")]
    [Tooltip("Duration of fade-in/out in seconds")]
    public float fadeDuration = 1f;
    [Tooltip("Curve to control fade shape")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private AudioSource audioSource;
    private Coroutine activeFadeCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        StartCoroutine(PlaySoundsSequentially());
    }

    IEnumerator PlaySoundsSequentially()
    {
        // Play first sound with fade in
        if (firstSound != null)
        {
            yield return FadeAudio(firstSound, 0f, 1f); // Fade in
            yield return new WaitForSeconds(firstSound.length);
            yield return FadeAudio(null, 1f, 0f); // Fade out
        }

        // Wait for specified delay
        yield return new WaitForSeconds(delayBetweenSounds);

        // Play second sound with fade in
        if (secondSound != null)
        {
            yield return FadeAudio(secondSound, 0f, 1f); // Fade in
            yield return new WaitForSeconds(secondSound.length);
            yield return FadeAudio(null, 1f, 0f); // Fade out
        }
    }

    IEnumerator FadeAudio(AudioClip clip, float startVolume, float endVolume)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / fadeDuration);
            float curveValue = fadeCurve.Evaluate(progress);
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, curveValue);
            yield return null;
        }

        if (endVolume <= 0.01f)
        {
            audioSource.Stop();
        }
    }

    // Public method to trigger playback manually
    public void PlaySequence()
    {
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }
        activeFadeCoroutine = StartCoroutine(PlaySoundsSequentially());
    }

    // Public method to stop playback
    public void StopSequence()
    {
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }
        StartCoroutine(FadeAudio(null, audioSource.volume, 0f));
    }
}