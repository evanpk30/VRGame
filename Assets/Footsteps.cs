using System.Collections;
using UnityEngine;

/// <summary>
/// Plays footstep sounds when the VR player is moving.
/// Attach this script to your VR player character.
/// </summary>
public class VRFootstepManager : MonoBehaviour
{
    [Header("Movement Detection")]
    [Tooltip("The transform that represents the VR headset position")]
    public Transform headTransform;
    
    [Tooltip("Minimum movement distance required to trigger a step")]
    public float stepThreshold = 0.05f;
    
    [Tooltip("Minimum time between footsteps")]
    public float stepCooldown = 0.4f;

    [Header("Audio Settings")]
    [Tooltip("AudioSource component for playing footstep sounds")]
    public AudioSource footstepAudioSource;
    
    [Tooltip("Array of footstep sound clips to randomize")]
    public AudioClip[] footstepSounds;
    
    [Tooltip("Volume of footstep sounds")]
    [Range(0.0f, 1.0f)]
    public float footstepVolume = 0.5f;

    // Private variables for tracking movement
    private Vector3 lastHeadPosition;
    private float timeSinceLastStep;
    private bool isGrounded = true;

    private void Start()
    {
        // Create AudioSource if not assigned
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.spatialBlend = 1.0f; // Make sound fully 3D
            footstepAudioSource.volume = footstepVolume;
            footstepAudioSource.playOnAwake = false;
        }

        // Use the headset position if available, otherwise use this transform
        if (headTransform == null)
        {
            // Try to find a VR camera or headset
            var vrCamera = FindObjectOfType<Camera>();
            if (vrCamera != null)
                headTransform = vrCamera.transform;
            else
                headTransform = transform;
        }

        lastHeadPosition = headTransform.position;
        timeSinceLastStep = 0f;
    }

    private void Update()
    {
        timeSinceLastStep += Time.deltaTime;

        // Calculate horizontal movement (ignoring vertical movement)
        Vector3 currentHeadPosition = headTransform.position;
        Vector3 horizontalMovement = new Vector3(
            currentHeadPosition.x - lastHeadPosition.x,
            0f,
            currentHeadPosition.z - lastHeadPosition.z
        );

        // Check if player has moved far enough for a footstep
        float movementDistance = horizontalMovement.magnitude;
        
        if (movementDistance >= stepThreshold && timeSinceLastStep >= stepCooldown && isGrounded)
        {
            PlayRandomFootstepSound();
            timeSinceLastStep = 0f;
        }

        lastHeadPosition = currentHeadPosition;
    }

    /// <summary>
    /// Plays a random footstep sound from the array of clips
    /// </summary>
    private void PlayRandomFootstepSound()
    {
        if (footstepSounds.Length == 0)
            return;

        int randomIndex = Random.Range(0, footstepSounds.Length);
        footstepAudioSource.clip = footstepSounds[randomIndex];
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.Play();
    }

    /// <summary>
    /// Call this method from ground detection systems to update grounded state
    /// </summary>
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }
}