using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ClimbingSystem : MonoBehaviour
{
    private CharacterController characterController;
    private Vector3 lastHandPosition;
    private bool isClimbing = false;
    private XRBaseInteractor climbingHand = null;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void StartClimbing(XRBaseInteractor hand)
    {
        if (isClimbing) return; // Prevent multiple hands interfering

        climbingHand = hand;
        lastHandPosition = hand.transform.position;
        isClimbing = true;
    }

    public void StopClimbing(XRBaseInteractor hand)
    {
        if (climbingHand == hand)
        {
            isClimbing = false;
            climbingHand = null;
        }
    }

    void Update()
    {
        if (isClimbing && climbingHand != null)
        {
            Vector3 handMovement = climbingHand.transform.position - lastHandPosition;
            characterController.Move(-handMovement); // Move player opposite to hand movement
            lastHandPosition = climbingHand.transform.position;
        }
    }
}
