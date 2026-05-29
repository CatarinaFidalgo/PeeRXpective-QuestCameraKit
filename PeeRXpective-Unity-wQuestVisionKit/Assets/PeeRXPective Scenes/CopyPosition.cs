using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosition : MonoBehaviour
{ //Should be named adjust visualization to the front of the expert user
    public Transform headTransform;
    public float distanceFromHead = 0.4f; // Distance from the head in meters
    public float y_Offset = 0.2f; // Offset from the head in meters
    public float x_Offset = 0.2f;

    public bool fixedPos = true;

    public float moveLerpSpeed = 7f;
    public Transform rightController;

    private Vector3 localOffset;
    private Vector3 worldOffset;
    Vector3 initialForward;
    Vector3 initialRight;
    Vector3 initialUp;

    private bool isMoving = false;
    private bool hasMoved = false;
    void Start()
    {

        initialForward = headTransform.forward;
        initialRight = headTransform.right;
        initialUp = headTransform.up;

        worldOffset = (initialForward * distanceFromHead)
                    + (initialRight * x_Offset)
                    - (initialUp * y_Offset);

        transform.position = headTransform.position + worldOffset;
    }
    void Update()
    {
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);

        if (triggerHeld)
        {
            isMoving = true;
            transform.position = Vector3.Lerp(transform.position, rightController.position, moveLerpSpeed * Time.deltaTime); ;
            transform.rotation = Quaternion.Slerp(transform.rotation, rightController.rotation, moveLerpSpeed * Time.deltaTime);
            
        }

        else
        {
            isMoving = false; 
                   
        }

        if (hasMoved && !isMoving)
        {
            DataCollection.Instance.LogTransformChange(
                transform.name,
                transform.position,
                transform.rotation.eulerAngles,
                transform.localScale.x
            );

        }
        
        hasMoved = isMoving;
    
    }
}
