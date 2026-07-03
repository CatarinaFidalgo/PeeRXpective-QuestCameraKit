using UnityEngine;
using System.Collections;

public class CameraFPSLimiter : MonoBehaviour
{
    [Tooltip("Target frames per second to capture.")]
    public float targetFPS = 15f;
    public Camera cam;

    void Start()
    {
        if (cam != null) 
                cam.enabled = false; 
        StartCoroutine(RenderAtCustomFPS());
    }

    private IEnumerator RenderAtCustomFPS()
    {
        while (true)
        {
            // PROTECTION 1: Only try to render if the camera is actually active in the scene
            if (cam != null && cam.gameObject.activeInHierarchy)
            {
                // PROTECTION 2: If the ConnectionEvent turned it on, instantly turn it back off
                if (cam.enabled)
                {
                    cam.enabled = false;
                }

                cam.Render(); // Take one picture
            }

            cam.Render(); // Take one picture
            yield return new WaitForSeconds(1f / targetFPS); 
        }
    }
}