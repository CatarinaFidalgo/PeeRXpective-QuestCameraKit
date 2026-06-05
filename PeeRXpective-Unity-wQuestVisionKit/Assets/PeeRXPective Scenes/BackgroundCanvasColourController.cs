using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackgroundCanvasColourController : MonoBehaviour
{
    [Header("UI Component Bindings")]
    [SerializeField] private TextMeshProUGUI titleLabel; // Drag your canvas TMP label here
    [SerializeField] private RawImage backgroundTexture;  // Drag your background RawImage/Panel here

    [Header("Canvas Highlight Colors Settings")]
    [SerializeField] public Color colorE = new Color(1f, 0.41f, 0.71f);     // Pink (Hot Pink default)
    [SerializeField] public Color colorT1 = new Color(0.11f, 0.56f, 1f);    // Blue (Deep Sky Blue)
    [SerializeField] public Color colorT2 = new Color(0.13f, 0.75f, 0.42f);  // Green
    [SerializeField] public Color colorT3 = new Color(1f, 0.55f, 0f);       // Orange (Dark Orange)
    [SerializeField] public Color colorT4 = new Color(1f, 0.92f, 0.016f);   // Yellow
    [SerializeField] public Color defaultColor = Color.black;               // Fallback / Disconnected

    private string lastCheckedText = "";

    void Start()
    {
        UpdateCanvasColor();
    }

    void Update()
    {
        // Only trigger color recalculation if the string text actually changed
        if (titleLabel != null && titleLabel.text != lastCheckedText)
        {   
            Debug.Log("Title label text changed, updating canvas color...");
            UpdateCanvasColor();
        }
    }

    public void UpdateCanvasColor()
    {
        Debug.Log("Updating canvas color based on title label text: " + titleLabel.text);

        if (titleLabel == null || backgroundTexture == null) return;

        lastCheckedText = titleLabel.text;

        if (lastCheckedText.Contains("E"))
        {
            backgroundTexture.color = colorE;
        }
        else if (lastCheckedText.Contains("T1"))
        {
            backgroundTexture.color = colorT1;
        }
        else if (lastCheckedText.Contains("T2"))
        {
            backgroundTexture.color = colorT2;
        }
        else if (lastCheckedText.Contains("T3"))
        {
            backgroundTexture.color = colorT3;
        }
        else if (lastCheckedText.Contains("T4"))
        {
            backgroundTexture.color = colorT4;
        }
        else
        {
            backgroundTexture.color = defaultColor;
        }
    }
}