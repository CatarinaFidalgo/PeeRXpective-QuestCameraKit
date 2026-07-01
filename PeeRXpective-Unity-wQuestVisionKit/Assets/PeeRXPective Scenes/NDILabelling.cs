using SimpleWebRTC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NDILabelling : MonoBehaviour
{
    private TextMeshProUGUI label;
    public bool labelled = false;

    void Start()
    {
        if (label == null)
        {
            label = GetComponentInChildren<TextMeshProUGUI>();
        }
        label.text = "Waiting transmission start...";
    }

    // Call this explicitly when a remote peer stream connects to this window
    public void AssignRemotePeerName(string remotePeerId)
    {
        // Use your extraction logic to clean the string if needed
        string cleanName = GetUserName(remotePeerId);

        label.text = cleanName;
        labelled = true;
        Debug.Log("[CAT] Window " + this.name + " labeled with remote sender: " + cleanName);
    }

    // Call this explicitly when a peer drops
    public void ClearLabel()
    {
        label.text = "Connection dropped...";
        labelled = false;
    }

    string GetUserName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        int open = s.IndexOf('(');
        int close = s.IndexOf(')', open + 1);

        if (open >= 0 && close > open)
            return s.Substring(open + 1, close - open - 1).Trim();

        return s.Trim();
    }
}

/*public class NDILabelling : MonoBehaviour
{
    private TextMeshProUGUI label;
    //public string clientName;
    public bool labelled = false;
    public bool connected = false;
    public WebRTCConnection webRTCConnection;

    void Start()
    {
        if (label == null)
        {
            label = GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log("Label component not assigned, found: " + label);
        }
        label.text = "Waiting transmission start...";
    }

    // Update is called once per frame
    void Update()
    {
        connected = webRTCConnection.WebRTCConnectionActive;

        if (labelled == false && connected == true)
        {
            label.text = "New Blop Name!";//webRTCConnection.LocalPeerId;
            label.text = webRTCConnection.LocalPeerId;
            labelled = true;
            Debug.Log("[CAT] Set label of " + this.name + " to: " + label.text);
            
        }

        if (labelled == true && connected == false)
        {
            label.text = "Connection dropped...";
            labelled = false;
        }

    }

    string GetUserName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        int open = s.IndexOf('(');
        int close = s.IndexOf(')', open + 1);

        if (open >= 0 && close > open)
            return s.Substring(open + 1, close - open - 1).Trim();

        // if no parentheses, just return original trimmed
        return s.Trim();
    }
}*/
