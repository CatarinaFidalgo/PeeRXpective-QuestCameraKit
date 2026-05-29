using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleControl : MonoBehaviour
{
    public enum Role { E, T1, T2, T3, T4 }
    public Role selectedRole;
    private Role currentRole;

    public GameObject expertViz;
    public GameObject traineeViz;
    void Start()
    {
        // Read the selected role from the Role Selection Scene
        //int savedRoleIndex = PlayerPrefs.GetInt("SelectedRole", 0); // default to 0 = E
        //selectedRole = (Role)savedRoleIndex;

        Debug.Log("This machine's user's Role: " + selectedRole);

        if (selectedRole == Role.E)
        {
            expertViz.SetActive(true);
            
            for (int i = 0; i < expertViz.transform.childCount; i++)
            {
                if (i == 1 || i == 3 || i == 5 || i == 7|| i == 8 || i == 9 )
                {
                    expertViz.transform.GetChild(i).gameObject.SetActive(false);
                }
            }

            traineeViz.SetActive(false);
        }
        else //selected role is Trainee
        {
            traineeViz.SetActive(true);

            for (int i = 0; i < traineeViz.transform.childCount; i++)
            {
                if (i == 1 || i == 3 || i == 5 || i == 7)
                {
                    traineeViz.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            expertViz.SetActive(false);
        }

        currentRole = selectedRole;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentRole != selectedRole)
        {
            if (selectedRole == Role.E)
            {
                expertViz.SetActive(true);
                traineeViz.SetActive(false);
            }
            else //selected role is Trainee
            {
                expertViz.SetActive(false);
                traineeViz.SetActive(true);
            }

            currentRole = selectedRole;
        }
    }
}
