using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NDILabelling : MonoBehaviour
{
    //public TextMeshPro textMeshPro;
    // Start is called before the first frame update
    public string ndiSetName;
    public TextMeshProUGUI textmp;
    public bool labelled = false;
    void Start()
    {
        //textmp = GetComponent<TextMeshProUGUI>();
        textmp.text = "Starting NDI...";
    }

    // Update is called once per frame
    void Update()
    {
        /*if (labelled == false)
        {
            textmp.text = "Looking for NDI source...";

            if (ndiSetName.con == true)
            {
                //textmp.text = ndiSetName.found;
                textmp.text = GetUserName(ndiSetName.found);
                labelled = true;
                Debug.Log("[NDI] Labelled NDI source as: " + ndiSetName.found);
            }
        }

        if (labelled == true && ndiSetName.con == false)
        {
            textmp.text = "Lost connection...";
            labelled = false;
            Debug.Log("[NDI] Lost connection to NDI source");
        }*/





        // if (ndiSetName._connected != null && labelled == false)
        // {
        //     textmp.text = ndiSetName.found;
        //     labelled = true;
        //     Debug.Log("[NDI] Labelled NDI source as: " + ndiSetName.found);
        // }

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
}
