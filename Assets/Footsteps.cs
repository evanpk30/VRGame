using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Plays footstep sounds when the VR player moves using controller input.
/// Attach this script to your VR player character.
/// </summary>
public class VRFootstepManager : MonoBehaviour
{
    [Header("Input Detection")]
    [Tooltip("The Input Device name for the left controller (leave empty to auto-detect)")]
    public string leftControllerName = "";

    [Tooltip("Minimum input magnitude required to trigger a step")]
    public float inputThreshold = 0.3f;

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

    [Tooltip("Scale the speed of footsteps with input magnitude")]
    public bool scaleWithSpeed = true;

    [Tooltip("Minimum cooldown time regardless of input magnitude")]
    public float minStepCooldown = 0.2f;

    // Debug settings
    [Header("Debug")]
    [Tooltip("Show debug information in the console")]
    public bool showDebugInfo = true;
    
    // Private variables
    private float timeSinceLastStep;
    private bool isGrounded = true;
    private InputDevice leftControllerDevice;
    private bool controllerInitialized = false;
    private float debugUpdateTimer = 0f;

    private void Start()
    {
        // Create AudioSource if not assigned
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.spatialBlend = 1.0f;
            footstepAudioSource.volume = footstepVolume;
            footstepAudioSource.playOnAwake = false;
        }

        // Start continuous controller detection
        StartCoroutine(InitializeControllerRoutine());
        
        timeSinceLastStep = 0f;
    }

    private IEnumerator InitializeControllerRoutine()
    {
        // Keep trying to find the controller until successful
        while (!controllerInitialized)
        {
            TryInitializeController();
            
            if (!controllerInitialized)
            {
                // Wait before trying again
                yield return new WaitForSeconds(1.0f);
            }
        }
    }

    private void TryInitializeController()
    {
        // Get all input devices
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        
        if (showDebugInfo)
        {
            Debug.Log($"Found {devices.Count} input devices");
            foreach (var device in devices)
            {
                Debug.Log($"Device: {device.name}, Characteristics: {device.characteristics}");
            }
        }

        // First try: Look for device with Left Controller characteristics
        foreach (var device in devices)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Left) != 0 &&
                (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                leftControllerDevice = device;
                controllerInitialized = true;
                
                if (showDebugInfo)
                    Debug.Log($"Found left controller by characteristics: {device.name}");
                
                return;
            }
        }
        
        // Second try: Look for device with specified name
        if (!string.IsNullOrEmpty(leftControllerName))
        {
            foreach (var device in devices)
            {
                if (device.name.Contains(leftControllerName))
                {
                    leftControllerDevice = device;
                    controllerInitialized = true;
                    
                    if (showDebugInfo)
                        Debug.Log($"Found left controller by name: {device.name}");
                    
                    return;
                }
            }
        }
        
        // Third try: Look for any device that supports primary2DAxis
        foreach (var device in devices)
        {
            // Check if the device supports the primary2DAxis control
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 _))
            {
                leftControllerDevice = device;
                controllerInitialized = true;
                
                if (showDebugInfo)
                    Debug.Log($"Found controller with joystick: {device.name}");
                
                return;
            }
        }

        if (showDebugInfo)
            Debug.Log("No suitable controller found. Will try again.");
    }

    private void Update()
    {
        timeSinceLastStep += Time.deltaTime;

        // Check if controller is initialized
        if (!controllerInitialized || !leftControllerDevice.isValid)
        {
            // Debug output (not too frequently)
            debugUpdateTimer += Time.deltaTime;
            if (showDebugInfo && debugUpdateTimer > 5.0f)
            {
                Debug.Log("Controller not initialized or no longer valid. Trying to reinitialize...");
                debugUpdateTimer = 0f;
                controllerInitialized = false;
                StartCoroutine(InitializeControllerRoutine());
            }
            return;
        }

        // Read the primary 2D axis from controller
        Vector2 joystickValue = Vector2.zero;
        if (leftControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue))
        {
            // Debug output (not too frequently)
            debugUpdateTimer += Time.deltaTime;
            if (showDebugInfo && debugUpdateTimer > 2.0f)
            {
                Debug.Log($"Joystick value: {joystickValue}, Magnitude: {joystickValue.magnitude}");
                debugUpdateTimer = 0f;
            }

            float inputMagnitude = joystickValue.magnitude;

            // Check if input exceeds threshold
            if (inputMagnitude >= inputThreshold && isGrounded)
            {
                // Calculate cooldown based on input magnitude
                float currentCooldown = stepCooldown;
                if (scaleWithSpeed)
                {
                    float speedFactor = Mathf.Lerp(1.0f, 0.5f, (inputMagnitude - inputThreshold) / (1.0f - inputThreshold));
                    currentCooldown = Mathf.Max(minStepCooldown, stepCooldown * speedFactor);
                }

                // Play footstep if cooldown has elapsed
                if (timeSinceLastStep >= currentCooldown)
                {
                    PlayRandomFootstepSound();
                    timeSinceLastStep = 0f;
                }
            }
        }
        else if (showDebugInfo && debugUpdateTimer > 5.0f)
        {
            Debug.Log("Could not read joystick value from controller.");
            debugUpdateTimer = 0f;
        }
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
        
        if (showDebugInfo)
            Debug.Log($"Playing footstep sound {randomIndex}");
    }

    /// <summary>
    /// Call this method from ground detection systems to update grounded state
    /// </summary>
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }
}