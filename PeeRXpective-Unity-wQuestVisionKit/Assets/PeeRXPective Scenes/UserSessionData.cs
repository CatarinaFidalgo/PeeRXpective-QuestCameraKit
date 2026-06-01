using UnityEngine;

public class UserSessionData : MonoBehaviour
{
    public static UserSessionData Instance { get; private set; }

    public string UserName = "User";
    public string UserColor = "White";

    // This is the formatted string your WebRTC connection will use
    public string FormattedPeerId => $"PeerID_{UserName}_{UserColor}";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persists when changing scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}