using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TriggerBoxDecision : MonoBehaviour
{

    public GameObject popupCanvas;
    public Text popupText;
    public GameObject objectToMove;
    public float pushForce = 20f;
    public ParticleSystem MagicParticleSystem;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            popupCanvas.SetActive(true);
            popupText.text = "Make a choice";
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
        popupCanvas.SetActive(false);
    }
}
