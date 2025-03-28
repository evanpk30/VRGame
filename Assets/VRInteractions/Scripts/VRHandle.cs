using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR adds a grippable handle to the lever. Makes the lever attachable to the VR controller.
/// </summary>
public class VRHandle : MonoBehaviour
{
    /// <summary>
    /// XR Interactable for the handle
    /// </summary>
    public XRGrabInteractable grabInteractable = new XRGrabInteractable();

    /// <summary>
    /// Object that is the spawn point of the handle joint
    /// </summary>
    public Transform HandlePosition;

    /// <summary>
    /// The joint that is spawned to connect the controller to the lever
    /// The prefab can be found in the prefabs folder
    /// </summary>
    public Transform HandleJointPrefab;

    /// <summary>
    /// Current joint object (null if there is no connection)
    /// </summary>
    private Transform JointObject;

    /// <summary>
    /// Sets the breakforce of the joint that connects the controller and the lever
    /// </summary>
    public float breakForces = 10;

    private bool bAttached = false;
    private XRBaseInteractor currentInteractor;

    void Awake()
    {
        // If not assigned in inspector, try to get the component
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        // Set up listeners for interaction events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        // Store the current interactor
        currentInteractor = args.interactorObject as XRBaseInteractor;

        // Trigger haptic feedback

        AddNewJoint(currentInteractor);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        Disconnect();
    }

    public void AddNewJoint(XRBaseInteractor _interactor)
    {
        // Ensure we have a Rigidbody to connect
        Rigidbody controllerBody = _interactor.transform.GetComponent<Rigidbody>();
        if (controllerBody == null) return;

        JointObject = Instantiate(HandleJointPrefab, HandlePosition.position, Quaternion.identity);
        JointObject.parent = transform;

        ConfigurableJoint cj = JointObject.GetComponent<ConfigurableJoint>();
        if (cj != null)
        {
            cj.connectedBody = controllerBody;
        }

        FixedJoint fj = JointObject.GetComponent<FixedJoint>();
        if (fj != null)
        {
            fj.connectedBody = GetComponent<Rigidbody>();
        }

        bAttached = true;
    }

    /// <summary>
    /// Disconnects the controller from the lever
    /// </summary>
    public void Disconnect()
    {
        if (JointObject != null)
        {
            Destroy(JointObject.gameObject);
        }

        currentInteractor = null;
        bAttached = false;
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}