
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;

//using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

// This script is attached to the GameObejct we want to interact with (e.g., a cube)
// It allows to move, rotate, and scale the object using the Oculus Touch controllers

// MAke sure object has a collider component and renderer to detect collisions with the controller
[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(Renderer))]

public class ControllerScript : MonoBehaviour
{
    [Header("Refs")]
    public Camera sceneCamera;                 // your main/center-eye cam
    public Transform rightController;          // drag RightHandAnchor (or your controller) here

    [Header("Interaction")]
    public float touchRadius;          // ~5cm proximity bubble
    public float moveLerpSpeed;          // how fast it follows the controller when moving
    public float scaleSpeed;            // scale units per second while holding trigger

    //public float originalDistance;     // distance from camera at start

    [Header("Debug")]
    public bool intersect = false;                  // 1 when controller overlaps, else 0

    [Tooltip("Optional UI graphic to tint (e.g., a full-rect Image under the Canvas).")]
    public Graphic highlightGraphic;
    Collider _col;
    Renderer _rend;

    [Tooltip("Distance to shift the object away from the controller to resolve overlaps.")]
    //public float shiftDistance = 0.2f;
    public Vector3 direction; // = transform.up;  
    private Vector3 fallbackPosition;
    private bool touching = false;

    //////////////////////////
    // private Vector3 lastPosition;
    // private Vector3 lastRotation;
    // private float lastScale;
    // private float logThreshold = 0.01f;

    private bool wasInteracting = false;
    private bool hasMoved = false;

    void Start()
    {
        direction = transform.right; // default direction to shift overlaps
        _col = GetComponent<Collider>();
        //_rend = GetComponent<Renderer>();

        // Start in front of the user
        if (sceneCamera != null)
            //transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * originalDistance;
            if (highlightGraphic)
                highlightGraphic.color = Color.white;

        fallbackPosition = transform.localPosition; // remember starting position

        ///////////////////////////
        // lastPosition = transform.position;
        // lastRotation = transform.rotation.eulerAngles;
        // lastScale = transform.localScale.x;
    }

    void Update()
    {
        if (rightController == null) return;

        touching = CheckCollision(touching);
        ///////////
        //intersect = touching;



        if (touching)
        {
            //_rend.material.color = Color.blue;
            if (highlightGraphic) highlightGraphic.color = Color.magenta;
            intersect = true;

        }
        else
        {
            //_rend.material.color = Color.white;
            if (highlightGraphic) highlightGraphic.color = Color.white;
            intersect = false;

        }
        
        // detect if we are currently interacting
        bool isInteracting = intersect && 
        (OVRInput.Get(OVRInput.RawButton.RThumbstickUp) ||
         OVRInput.Get(OVRInput.RawButton.RThumbstickDown) ||
         OVRInput.Get(OVRInput.RawButton.A));

        if (intersect)
        {
            wasInteracting = true; // mark that we have been interacting

            if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
            {
                //Debug.Log("Pressing Right Thumbstick Up. Scaling object up.");
                transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
                if (transform.localScale.x > 3f) transform.localScale = Vector3.one * 3f; // clamp max size

                //LogTransformChange(string objectName, Vector3 position, Vector3 rotation, float scale)
                //DataCollection.Instance.LogTransformChange(transform.name, transform.position, transform.rotation.eulerAngles, transform.localScale.x);
            
                hasMoved = true;

            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
            {
                //Debug.Log("Pressing Right Thumbstick Down. Scaling object down.");
                transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime;
                if (transform.localScale.x < 0.25f) transform.localScale = Vector3.one * 0.25f; // clamp min size
                // DataCollection.Instance.LogTransformChange(transform.name, transform.position, transform.rotation.eulerAngles, transform.localScale.x);
                hasMoved = true;
            }

            // --- Move & rotate while holding A ---
            else if (OVRInput.Get(OVRInput.RawButton.A))
            {
                //Debug.Log("Pressing A. Moving object.");
                transform.position = Vector3.Lerp(transform.position, rightController.position, moveLerpSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, rightController.rotation, moveLerpSpeed * Time.deltaTime);

                //DataCollection.Instance.LogTransformChange(transform.name, transform.position, transform.rotation.eulerAngles, transform.localScale.x);

                //if OVRInput A is released, check for overlaps and resolve
                // if (OVRInput.GetUp(OVRInput.RawButton.A))
                // ResolveOverlaps();

                hasMoved = true;
            }

            // current object we are touching goes back to center
            else if (OVRInput.Get(OVRInput.RawButton.B)) //  if (OVRInput.GetUp(OVRInput.RawButton.A))
            {
                transform.localPosition = fallbackPosition;
                transform.localRotation = Quaternion.identity;

                hasMoved = true;
            }
            
            //DataCollection.Instance.LogTransformChange(transform.name, transform.position, transform.rotation.eulerAngles, transform.localScale.x);
            
        }

        else if (wasInteracting && hasMoved)
        {
            DataCollection.Instance.LogTransformChange(
                transform.name,
                transform.position,
                transform.rotation.eulerAngles,
                transform.localScale.x
            );

            wasInteracting = false; // reset interaction flag
            hasMoved = false;

            // // Update last known values
            // lastPosition = transform.position;
            // lastRotation = transform.rotation.eulerAngles;
            // lastScale = transform.localScale.x;

            //DataCollection.Instance.LogTransformChange(transform.name, transform.position, transform.rotation.eulerAngles, transform.localScale.x);
         
        }

           

    }


    bool CheckCollision(bool touching)
    {
        // --- Overlap test vs controller position (world space) ---
        touching = _col.bounds.SqrDistance(rightController.position) <= (touchRadius * touchRadius);

        // Collider[] hits = Physics.OverlapSphere(rightController.position, touchRadius);
        // Collider closest = null;
        // float bestSqr = float.MaxValue;

        // for (int i = 0; i < hits.Length; i++)
        // {
        //     var c = hits[i];
        //     if (c == null) continue;
        //     float sqr = (c.bounds.ClosestPoint(rightController.position) - rightController.position).sqrMagnitude;
        //     if (sqr < bestSqr) { bestSqr = sqr; closest = c; }
        // }

        // Only consider touching if this object's collider is the closest one
        //return touching = (closest == _col) && bestSqr <= (touchRadius * touchRadius);
        return touching;
    }
    
    // private bool HasTransformChanged()
    // {
    //     // Check movement
    //     if (Vector3.Distance(transform.position, lastPosition) > logThreshold)
    //         return true;

    //     // Check rotation
    //     if (Vector3.Distance(transform.rotation.eulerAngles, lastRotation) > logThreshold)
    //         return true;

    //     // Check scale
    //     if (Mathf.Abs(transform.localScale.x - lastScale) > logThreshold)
    //         return true;

    //     return false;
    // }

    // void ResolveOverlaps()
    // {
    //     // After releasing the object, check for overlaps with other objects and resolve them

    //     if (_col == null)
    //     {
    //         Debug.LogWarning("No collider found on the object to resolve overlaps.");
    //         return;
    //     }


    //     // Collider[] overlappingColliders = Physics.OverlapBox(_col.bounds.center, _col.bounds.extents, transform.rotation);
    //     // foreach (Collider col in overlappingColliders)
    //     // {
    //     //     Debug.Log("Overlapping with: " + col.name);

    //     //     if (col != _col)
    //     //     {
    //     //         // Simple resolution: move the object away along the vector between their centers
    //     //         //Vector3 direction = (transform.position - col.bounds.center).normalized;
    //     //         //float distance = _col.bounds.extents.magnitude + col.bounds.extents.magnitude;
    //     //         transform.position = _col.bounds.center + direction * shiftDistance;

    //     //     }
    //     // }

    //     if (_col == null) return;

    //     // find overlaps with the bounds AABB (cheap). You can change to OverlapBoxNonAlloc if you prefer.
    //     Collider[] hits = Physics.OverlapBox(_col.bounds.center, _col.bounds.extents, transform.rotation, ~0, QueryTriggerInteraction.Ignore);

    //     foreach (var other in hits)
    //     {
    //         if (other == null || other == _col) continue;

    //         // if their centers are very close (or identical), push this object to the side
    //         Vector3 between = transform.position - other.bounds.center;
    //         float betweenMag = between.magnitude;
    //         const float small = 1e-4f;

    //         if (betweenMag < small)
    //         {
    //             // centers coincide (or almost). Shift by fixed inspector distance along chosen axis.
    //             // if (pushDir.sqrMagnitude < 1e-6f) pushDir = Vector3.right; // fallback
    //             // pushDir = pushDir.normalized;

    //             transform.position += direction * shiftDistance;
    //         }
    //         else
    //         {
    //             // simple non-precise push-away so bounding-spheres just touch (your earlier idea)
    //             Vector3 dir = between / betweenMag; // normalized
    //             float separation = _col.bounds.extents.magnitude + other.bounds.extents.magnitude + 0.01f;
    //             transform.position = other.bounds.center + dir * separation;
    //         }
    //     }
    // }


}
