using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ClimbableWall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.name} entered climbing area!"); // Debug log

        XRDirectInteractor hand = other.GetComponent<XRDirectInteractor>();
        if (hand)
        {
            Debug.Log($"{hand.name} is a valid climbing hand!"); // Debug log

            ClimbingSystem climbingSystem = hand.transform.root.GetComponent<ClimbingSystem>();
            if (climbingSystem)
            {
                climbingSystem.StartClimbing(hand);
                Debug.Log("Climbing started!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        XRDirectInteractor hand = other.GetComponent<XRDirectInteractor>();
        if (hand)
        {
            ClimbingSystem climbingSystem = hand.transform.root.GetComponent<ClimbingSystem>();
            if (climbingSystem)
            {
                climbingSystem.StopClimbing(hand);
                Debug.Log("Climbing stopped!");
            }
        }
    }
}


