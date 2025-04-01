using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR gripped button. Requires XR Interactable. This button responds to being "clicked" rather than a physical press.
/// </summary>
[RequireComponent(typeof(XRBaseInteractable))]
public class VRGrippedButton : MonoBehaviour
{
    /// <summary>
    /// Animation that makes the button press down
    /// </summary>
    public Animation ButtonAnim;

    /// <summary>
    /// XR Interactable component
    /// </summary>
    private XRBaseInteractable Interactable;

    void OnEnable()
    {
        Interactable = GetComponent<XRBaseInteractable>();
        if (Interactable == null)
            Debug.LogError("XR Interactable is null");

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = true; // This button should only work as a trigger

        // Set up interaction callbacks
        Interactable.onSelectEntered.AddListener(OnGrabbed);
    }

    void OnDisable()
    {
        // Remove interaction callbacks
        if (Interactable != null)
            Interactable.onSelectEntered.RemoveListener(OnGrabbed);
    }

    void OnTriggerEnter(Collider _collider)
    {
        // Check if the interactable is selected
        if (Interactable != null && Interactable.isSelected)
            ActivateButton(_collider.attachedRigidbody);
    }

    /// <summary>
    /// Triggered when the button is grabbed
    /// </summary>
    /// <param name="interactor">The interactor selecting the object</param>
    private void OnGrabbed(XRBaseInteractor interactor)
    {
        if (interactor != null)
        {
            ActivateButton(interactor.transform.GetComponent<Rigidbody>());
        }
    }

    /// <summary>
    /// Triggers the button if the controllers action key is down
    /// </summary>
    /// <param name="_controllerBody">Controller body.</param>
    public void ActivateButton(Rigidbody _controllerBody)
    {
        if (_controllerBody == null)
            return;

        XRBaseController controller = _controllerBody.gameObject.GetComponent<XRBaseController>();
        if (controller == null)
            return;

        // Check if the object is currently selected
        if (Interactable.isSelected)
        {
            if (ButtonAnim != null)
                ButtonAnim.Play();
        }
    }
}