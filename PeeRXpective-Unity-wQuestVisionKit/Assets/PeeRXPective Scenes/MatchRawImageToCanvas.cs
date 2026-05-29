using UnityEngine;
using UnityEngine.UI;

public class MatchRawImageToCanvas : MonoBehaviour
{
    void Start()
    {
        AdjustSize();
    }

    void AdjustSize()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("No Canvas component found on this GameObject.");
            return;
        }

        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        RawImage rawImage = GetComponentInChildren<RawImage>();

        if (rawImage != null)
        {
            RectTransform rawImageRectTransform = rawImage.GetComponent<RectTransform>();

            // Match the size of the RawImage to the size of the Canvas
            rawImageRectTransform.sizeDelta = new Vector2(canvasRectTransform.rect.width, canvasRectTransform.rect.height);

            // Optionally, set the anchors to stretch to fill the canvas
            // rawImageRectTransform.anchorMin = new Vector2(0, 0);
            // rawImageRectTransform.anchorMax = new Vector2(1, 1);
            // rawImageRectTransform.offsetMin = Vector2.zero;
            // rawImageRectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            Debug.LogWarning("No RawImage found as a child of the Canvas.");
        }
    }
}
