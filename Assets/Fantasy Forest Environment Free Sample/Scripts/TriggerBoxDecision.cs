using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class TriggerBoxDecision : MonoBehaviour
{

    public GameObject popupCanvas;
    public Text popupText;
    public GameObject objectToMove;
    public float pushForce = 20f;
    public ParticleSystem MagicParticleSystem;
    public XRBaseController Controller;
    private XRDirectInteractor directInteractor;
    public GameObject rayInteractor;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            popupCanvas.SetActive(true);
            directInteractor = Controller.GetComponent<XRDirectInteractor>();
            //set all the ray interactor needs to true, also set the direct interactor to false as to not interfere
            rayInteractor.SetActive(true);
            directInteractor.enabled = false;
            string stop = "stopTime";
            Invoke(stop, 0.5f);
            
        }

    }

    public void onChoiceA()
    {
        Debug.Log("Still Running");

        if (MagicParticleSystem != null)
        {
            // Ensure the GameObject is active
            MagicParticleSystem.gameObject.SetActive(true);

            MagicParticleSystem.Play();
            Debug.Log("Particle system started playing.");
        }
        else
        {
            Debug.LogError("MagicParticleSystem is not assigned!");
        }

        closePopup();
    }

    public void onChoiceB()
    {
        // Push the object forward
        Rigidbody rb = objectToMove.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force in the object's forward direction
            rb.AddForce(objectToMove.transform.forward * pushForce, ForceMode.Impulse);
            Debug.Log("Object pushed forward with force: " + pushForce);
        }
        else
        {
            Debug.LogError("No Rigidbody component found on objectToMove!");
        }

        closePopup();
    }


    public void closePopup()
    {
        Time.timeScale = 1;
        rayInteractor.SetActive(false);
        directInteractor.enabled = true;
        popupCanvas.SetActive(false);
    }

    public void stopTime()
    {
        Time.timeScale = 0;
    }
}
