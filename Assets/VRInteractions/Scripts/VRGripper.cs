using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR gripper. Component goes on the controller. It is used to make physics joint connections with objects in the scene. Also handles haptic feedback along those connections
/// </summary>
public class VRGripper : MonoBehaviour
{

    /// <summary>
    /// The current XR controller this component is attached to
    /// </summary>
    private XRBaseController CurrentController;

    /// <summary>
    /// Optional audio source for playing surface hits
    /// </summary>
    private AudioSource CurrentAudio;

    /// <summary>
    /// List of controllers used by VRInteractable, can be accessed via the static method GetControllers
    /// </summary>
    private static List<XRBaseController> ControllerList = new List<XRBaseController>();

    /// <summary>
    /// Gets the controllers currently managed by grippers
    /// </summary>
    /// <returns>List of XR Controllers</returns>
    public static List<XRBaseController> GetControllers()
    {
        return new List<XRBaseController>(ControllerList);
    }

    /// <summary>
    /// Used to prevent multiple collision reactions
    /// </summary>
    private bool isColliding = false;

    /// <summary>
    /// Is this controller currently gripping something?
    /// </summary>
    private bool isGripping = false;

    /// <summary>
    /// How long the haptic pulse lasts
    /// </summary>
    public float VibrationLength = 0.1f;

    /// <summary>
    /// The haptic pulse strength
    /// </summary>
    public float HapticPulseStrength = 0.5f;

    void Awake()
    {
        // Cache local components
        CurrentController = GetComponent<XRBaseController>();
        CurrentAudio = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Add this controller to the static list
        if (CurrentController != null && !ControllerList.Contains(CurrentController))
            ControllerList.Add(CurrentController);

        // Set up interaction callbacks if possible
        var interactor = GetComponent<XRBaseInteractor>();
        if (interactor != null)
        {
            interactor.selectEntered.AddListener(OnGrabbed);
            interactor.selectExited.AddListener(OnReleased);
        }
    }

    void OnDisable()
    {
        // Remove this controller from the static list
        if (CurrentController != null && ControllerList.Contains(CurrentController))
            ControllerList.Remove(CurrentController);

        // Remove interaction callbacks
        var interactor = GetComponent<XRBaseInteractor>();
        if (interactor != null)
        {
            interactor.selectEntered.RemoveListener(OnGrabbed);
            interactor.selectExited.RemoveListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGripping = true;
        HapticVibration(HapticPulseStrength, VibrationLength);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isGripping = false;
    }

    void OnCollisionEnter(Collision _collision)
    {
        // If we're not already holding something or colliding then play our response
        if (!isColliding && !isGripping)
        {
            StartCoroutine(LongVibration(HapticPulseStrength, VibrationLength));

            if (CurrentAudio != null)
            {
                CurrentAudio.Play();
            }

            isColliding = true;
        }
    }

    /// <summary>
    /// Tracks if a haptic response is currently active
    /// </summary>
    private bool isVibrating = false;

    /// <summary>
    /// Generates a long vibration effect
    /// </summary>
    /// <param name="_strength">Strength of vibration (0-1)</param>
    /// <param name="_duration">Duration of vibration</param>
    IEnumerator LongVibration(float _strength, float _duration)
    {
        if (isVibrating)
            yield break;

        isVibrating = true;

        _strength = Mathf.Clamp(_strength, 0, 1);
        float elapsedTime = 0;

        while (elapsedTime < _duration)
        {
            // Use XR controller haptic methods
            CurrentController.SendHapticImpulse(_strength, Time.fixedDeltaTime);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // finished
        isVibrating = false;
    }

    /// <summary>
    /// Trigger a haptic pulse with given strength
    /// </summary>
    /// <param name="_strength">Strength of vibration (0-1)</param>
    void HapticPulse(float _strength)
    {
        CurrentController.SendHapticImpulse(_strength, 0.1f);
    }

    /// <summary>
    /// Vibrates the controller at given strength for given duration
    /// </summary>
    /// <param name="_strength">Strength of vibration</param>
    /// <param name="_duration">Duration of vibration</param>
    public void HapticVibration(float _strength, float _duration)
    {
        StartCoroutine(LongVibration(_strength, _duration));
    }

    void OnCollisionExit(Collision _collision)
    {
        isColliding = false;
    }
}