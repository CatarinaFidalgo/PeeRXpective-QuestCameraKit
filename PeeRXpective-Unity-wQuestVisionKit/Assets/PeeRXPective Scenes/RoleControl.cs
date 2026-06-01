using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleControl : MonoBehaviour
{
    public enum Role { E, T1, T2, T3, T4 }
    public Role selectedRole;

    void Start()
    {

        Debug.Log("This machine's user's Role: " + selectedRole);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
