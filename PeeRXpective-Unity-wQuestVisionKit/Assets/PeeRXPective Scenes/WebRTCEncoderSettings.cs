using UnityEngine;
using SimpleWebRTC;

public class WebRTCEncoderSettings : MonoBehaviour
{
    // This creates a global reference so WebRTCManager can easily find it
    public static WebRTCEncoderSettings Instance;

    [Header("Encoder Settings")]
    [Tooltip("Target bitrate in bits per second (e.g., 500000 = 500 kbps)")]
    public int maxBitrate = 500000;

    [Tooltip("Target frames per second (e.g., 15)")]
    public int maxFramerate = 15;

    private void Awake()
    {
        // Set up the singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}