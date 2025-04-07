using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable), typeof(AudioSource))]
public class GrabbableAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip grabSound;
    [SerializeField] private AudioClip releaseSound;
    [SerializeField] [Range(0, 1)] private float volume = 0.8f;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] [Range(0.5f, 1.5f)] private float pitchRangeMin = 0.9f;
    [SerializeField] [Range(0.5f, 1.5f)] private float pitchRangeMax = 1.1f;

    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Configure audio source
        audioSource.spatialBlend = 1.0f; // 3D sound
        audioSource.playOnAwake = false;

        // Set up event listeners
        grabInteractable.selectEntered.AddListener(PlayGrabSound);
        grabInteractable.selectExited.AddListener(PlayReleaseSound);
    }

    private void PlayGrabSound(SelectEnterEventArgs arg)
    {
        if (grabSound == null) return;
        PlaySound(grabSound);
    }

    private void PlayReleaseSound(SelectExitEventArgs arg)
    {
        if (releaseSound == null) return;
        PlaySound(releaseSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (randomizePitch)
            audioSource.pitch = Random.Range(pitchRangeMin, pitchRangeMax);
        
        audioSource.PlayOneShot(clip, volume);
    }

    void OnDestroy()
    {
        // Clean up event listeners
        grabInteractable.selectEntered.RemoveListener(PlayGrabSound);
        grabInteractable.selectExited.RemoveListener(PlayReleaseSound);
    }
}