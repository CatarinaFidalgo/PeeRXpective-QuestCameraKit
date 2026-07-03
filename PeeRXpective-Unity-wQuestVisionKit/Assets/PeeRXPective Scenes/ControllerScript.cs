
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider))]


public class ControllerScript : MonoBehaviour
{
    [Header("Refs")]
    public Camera sceneCamera;                 // your main/center-eye cam
    public Transform rightController;          // drag RightHandAnchor (or your controller) here

    [Header("Interaction")]
    public float touchRadius;          // ~5cm proximity bubble
    public float moveLerpSpeed;          // how fast it follows the controller when moving
    public float scaleSpeed;            // scale units per second while holding trigger

    
    [Header("Debug")]
    public bool intersect = false;                  // 1 when controller overlaps, else 0

    [Tooltip("Optional UI graphic to tint (e.g., a full-rect Image under the Canvas).")]
    public Graphic highlightGraphic;
    //private Graphic highlightGraphicDefault;
    private Color defaultColor;
    Collider _col;
    Renderer _rend;

    [Tooltip("Distance to shift the object away from the controller to resolve overlaps.")]
    //public float shiftDistance = 0.2f;
    public Vector3 direction; // = transform.up;  
    private Vector3 fallbackPosition;
    private bool touchingCanvas = false;
    private bool pressedButton = false;

    private bool wasInteracting = false;
    private bool hasMoved = false;

    public Calibration calibration;
    private string changeType = ""; 

    void Start()
    {
        direction = transform.right; // default direction to shift overlaps
        _col = GetComponent<Collider>();
        //_rend = GetComponent<Renderer>();

        // Start in front of the user
        if (sceneCamera != null)
            if (highlightGraphic)
                defaultColor = highlightGraphic.color;

        fallbackPosition = transform.localPosition; // remember starting position

    }

    void Update()
    {
        if (rightController == null)
        {
            Debug.LogWarning("Right controller reference not set.");
            return;
        }

        if (highlightGraphic == null)
        {
            Debug.LogWarning("Highlight graphic reference not set.");
        }

        touchingCanvas = CheckCollision(touchingCanvas);


        if (touchingCanvas)
        {
            //Debug.Log("touchingCanvas object: " + transform.name);
            highlightGraphic.color = Color.blue;          

        }
        else
        {
            highlightGraphic.color = defaultColor;
            //highlightGraphic.color = Color.blue;
        }

        pressedButton = ((OVRInput.Get(OVRInput.RawButton.RThumbstickUp) || OVRInput.Get(OVRInput.RawButton.RThumbstickDown) || OVRInput.Get(OVRInput.RawButton.A) || OVRInput.Get(OVRInput.RawButton.B)) && calibration.isCalibrated);

        /*if ((OVRInput.Get(OVRInput.RawButton.RThumbstickUp) || OVRInput.Get(OVRInput.RawButton.RThumbstickDown) || OVRInput.Get(OVRInput.RawButton.A) || OVRInput.Get(OVRInput.RawButton.B)) && calibration.isCalibrated)
        {            
           //Debug.Log("Pressed a button: (up) " + OVRInput.Get(OVRInput.RawButton.RThumbstickUp) + ", (down) " + OVRInput.Get(OVRInput.RawButton.RThumbstickDown) + ", (A) " + OVRInput.Get(OVRInput.RawButton.A));
           pressedButton = true;
        }*/

        if (touchingCanvas & pressedButton)
        {
            //bool isInteracting = true;
            //Debug.Log("Manipulating object: " + transform.name);

            wasInteracting = true;

            if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
            {
                //Debug.Log("Pressing Right Thumbstick Up. Scaling object up.");
                transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
                if (transform.localScale.x > 3f) transform.localScale = Vector3.one * 3f; // clamp max size

                changeType = "Scale(up)";
                hasMoved = true;

            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
            {
                //Debug.Log("Pressing Right Thumbstick Down. Scaling object down.");
                transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime;
                if (transform.localScale.x < 0.25f) transform.localScale = Vector3.one * 0.25f; // clamp min size

                changeType = "Scale(down)";
                hasMoved = true;
            }
            else if (OVRInput.Get(OVRInput.RawButton.A))
            {
                //Debug.Log("Pressing A. Moving object.");
                transform.position = Vector3.Lerp(transform.position, rightController.position, moveLerpSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, rightController.rotation, moveLerpSpeed * Time.deltaTime);

                changeType = "Move_i(position/rotation)";
                hasMoved = true;
            }
            else if (OVRInput.Get(OVRInput.RawButton.B))
            {
                transform.localPosition = fallbackPosition;
                transform.localRotation = Quaternion.identity;

                hasMoved = true;
            }
        }

        else if (wasInteracting && hasMoved && calibration.isCalibrated)
        {
            // Convert the world coordinates to calibrated referential
            Vector3 calibratedPos = calibration.WorldToTablePosition(transform.position);
            Vector3 calibratedRot = calibration.WorldToTableRotation(transform.rotation).eulerAngles;

            DataCollection.Instance.LogTransformChange(
                transform.name,
                changeType,
                calibratedPos,
                calibratedRot,
                transform.localScale.x
            );

            wasInteracting = false; // reset interaction flag
            hasMoved = false;

        }

    }

    /*private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay with: " + other.gameObject.name);
        touchingCanvas = true;
        *//*// If the collider entering our space is the rightController or part of its hierarchy tree
        if (other.transform == rightController || other.transform.root == rightController.root)
        {
            touchingCanvas = true;
        }*//*
    }

    private void OnTriggerExit(Collider other)
    {
        touchingCanvas = false;
        *//*if (other.transform == rightController || other.transform.root == rightController.root)
        {
            touchingCanvas = false;
        }*//*
    }*/

    bool CheckCollision(bool touching)
    {
        if (rightController == null) return false;

        // 1. Calculate the raw distance between the controller and the center of this UI element
        float distance = Vector3.Distance(transform.position, rightController.position);

        // 2. If the controller is within your touchRadius (e.g., 0.05f for 5cm), return true
        return distance <= touchRadius;
    }


}