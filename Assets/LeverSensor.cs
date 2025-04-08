using System.Collections;
using UnityEngine;

public class SimpleDoorController : MonoBehaviour
{
    public Transform doorTransform;
    public Vector3 openOffset = new Vector3(0, 2, 0);
    public float moveSpeed = 1f;
    
    private Vector3 closedPosition;
    private Vector3 openPosition;
    
    void Start()
    {
        if (doorTransform == null) doorTransform = transform;
        closedPosition = doorTransform.position;
        openPosition = closedPosition + openOffset;
    }
    
    public void OpenDoor()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(openPosition));
    }
    
    public void CloseDoor()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(closedPosition));
    }
    
    IEnumerator MoveDoor(Vector3 targetPosition)
    {
        while (Vector3.Distance(doorTransform.position, targetPosition) > 0.01f)
        {
            doorTransform.position = Vector3.MoveTowards(
                doorTransform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}