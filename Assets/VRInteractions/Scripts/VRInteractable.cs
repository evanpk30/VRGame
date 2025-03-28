using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Base class for interactables. Tracks whether the object is interactable or not and contains facilities for managing colliders.
/// Collider management is useful for if you want to temporarily stop the controllers or the character from colliding with the interactable object
/// </summary>
public class VRInteractable : MonoBehaviour
{

    public bool Interactable = true;

    /// <summary>
    /// Requires a rigidbody
    /// </summary>
    private Rigidbody rb3d;

    /// <summary>
    /// Cached colliders for disabling collisions
    /// </summary>
    private List<Collider> mColliders3D;

    void Awake()
    {
        UpdateColliders3D();
        mCache = Interactable;
    }

    /// <summary>
    /// Updates the cache of which colliders are in this object's hierarchy.
    /// </summary>
    public void UpdateColliders3D()
    {
        rb3d = gameObject.GetComponent<Rigidbody>();

        mColliders3D = new List<Collider>();

        if (rb3d != null)
        {
            mColliders3D.AddRange(rb3d.gameObject.GetComponentsInChildren<Collider>());
            mColliders3D.Add(rb3d.gameObject.GetComponent<Collider>());
        }
        else
        {
            mColliders3D.AddRange(GetComponentsInChildren<Collider>());
            mColliders3D.Add(GetComponent<Collider>());
        }
    }

    /// <summary>
    /// Ignores the colliders of the given rigidbody
    /// </summary>
    /// <param name="_rigidbody">Rigidbody.</param>
    public void IgnoreColliders(Rigidbody _rigidbody)
    {
        Collider[] colliders = _rigidbody.GetComponentsInChildren<Collider>();
        IgnoreColliders3D(colliders, mColliders3D.ToArray());
    }

    /// <summary>
    /// Ignores the colliders of the given transform
    /// </summary>
    /// <param name="_object">Object.</param>
    public void IgnoreColliders(Transform _object)
    {
        Collider[] colliders = _object.GetComponentsInChildren<Collider>();
        IgnoreColliders3D(colliders, mColliders3D.ToArray());
    }

    /// <summary>
    /// Removes the physics ignore for the given rigidbody
    /// </summary>
    /// <param name="_rigidbody">Rigidbody.</param>
    public void RemoveIgnoreColliders(Rigidbody _rigidbody)
    {
        Collider[] colliders = _rigidbody.GetComponentsInChildren<Collider>();
        IgnoreColliders3D(colliders, mColliders3D.ToArray(), false);
    }

    /// <summary>
    /// Removes the physics ignore for the given transform
    /// </summary>
    /// <param name="_object">Object.</param>
    public void RemoveIgnoreColliders(Transform _object)
    {
        Collider[] colliders = _object.GetComponentsInChildren<Collider>();
        IgnoreColliders3D(colliders, mColliders3D.ToArray(), false);
    }

    /// <summary>
    /// Allows prevention of collision between this object and your controllers colliders
    /// </summary>
    /// <param name="_colliders">Colliders of object we want to ignore collisions with</param>
    /// <param name="_otherColliders">Colliders to ignore</param>
    /// <param name="_ignore">Whether to ignore or restore collisions</param>
    public static void IgnoreColliders3D(Collider[] _colliders, Collider[] _otherColliders, bool _ignore = true)
    {
        foreach (Collider col in _colliders)
        {
            foreach (Collider otherCol in _otherColliders)
            {
                if (otherCol == null || col == null)
                {
                    continue;
                }

                Physics.IgnoreCollision(col, otherCol, _ignore);
            }
        }
    }

    /// <summary>
    /// Ignores colliders for all of the active XR controllers
    /// </summary>
    void IgnoreAllControllerColliders()
    {
        foreach (XRBaseController controller in VRGripper.GetControllers())
        {
            IgnoreColliders(controller.transform);
        }
    }

    /// <summary>
    /// Removes the physics ignore for the XR controllers
    /// </summary>
    void RemoveIgnoreAllControllerColliders()
    {
        foreach (XRBaseController controller in VRGripper.GetControllers())
        {
            RemoveIgnoreColliders(controller.transform);
        }
    }

    /// <summary>
    /// Tracks changes to interactable state
    /// </summary>
    private bool mCache;

    public void Update()
    {
        if (mCache != Interactable)
        {
            if (!Interactable)
                IgnoreAllControllerColliders();
            else
                RemoveIgnoreAllControllerColliders();

            mCache = Interactable;
        }
    }
}