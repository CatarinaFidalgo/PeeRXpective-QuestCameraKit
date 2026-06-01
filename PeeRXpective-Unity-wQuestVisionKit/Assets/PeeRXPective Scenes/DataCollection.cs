using System;
using System.IO;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android; // <--- Needed for Permission class
#endif

public class DataCollection : MonoBehaviour
{
    public static DataCollection Instance;  // Singleton access
    public enum Role { E, T1, T2, T3, T4 }
    public Role role;
    public string pID;

    private string folderPath;
    private string filePath;
    private string participantID;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Request storage permission on Android
    #if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
    #endif

            participantID = role.ToString() + "_pid" + pID;


            // Path for CSVs
    #if UNITY_ANDROID && !UNITY_EDITOR
            folderPath = Path.Combine("/sdcard/Documents", "DataCollection");
    #else
            folderPath = Path.Combine(Application.persistentDataPath, "DataCollection");
    #endif
        
        CreateFolder(folderPath);

        //Debug.Log("DataCollection folder path: " + folderPath);

        string timestamp = DateTime.Now.ToString("MM_dd_HH_mm");
        filePath = Path.Combine(folderPath, $"{timestamp}__{participantID}__ScreenOrganizationLogs.csv");

        //Debug.Log("DataCollection file path: " + filePath);

        string[] header = {
            "TimeStamp", "ParticipantID", "ObjectName",
            "PosX", "PosY", "PosZ",
            "RotX", "RotY", "RotZ",
            "Scale"
        };

        CreateFile(filePath, header);
    }


    public void LogTransformChange(string objectName, Vector3 position, Vector3 rotation, float scale)
    {
        string[] data = {
            DateTime.Now.ToString("HH:mm:ss.fff"),
            "id_" + participantID,
            "view_" + objectName,
            position.x.ToString("F2"),
            position.y.ToString("F2"),
            position.z.ToString("F2"),
            rotation.x.ToString("F2"),
            rotation.y.ToString("F2"),
            rotation.z.ToString("F2"),
            scale.ToString("F2")
        };

        Debug.Log($"Logging Transform Change: {string.Join(", ", data)}");
        AppendLine(filePath, data);
    }

    /// <summary>
    ///   Creates a folder if it doesn't exist already.
    /// </summary>
    /// <param name="path"></param>
    private void CreateFolder(string path)
    {
        var folders = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        /***   prev ***/
        // string currentPath = "/";

        // Determine the correct starting root based on the operating system
        string currentPath = "";

        #if UNITY_ANDROID && !UNITY_EDITOR
            currentPath = "/"; // Keep the Android root slash
        #else
                // On Windows, the first element will be "C:" -> let it be the root without a leading slash
                if (folders.Length > 0 && folders[0].Contains(":"))
                {
                    currentPath = folders[0] + "\\";
                    // Remove the drive letter from our loop array so we don't process it twice
                    string[] remainingFolders = new string[folders.Length - 1];
                    Array.Copy(folders, 1, remainingFolders, 0, folders.Length - 1);
                    folders = remainingFolders;
                }
                else
                {
                    currentPath = "/";
                }
        #endif

        /******************/

        foreach (var folder in folders)
        {
            currentPath = Path.Combine(currentPath, folder);
            if (!Directory.Exists(currentPath))
            {
                //Debug.Log("Creating folder: " + currentPath);
                Directory.CreateDirectory(currentPath);
                //Debug.Log("Created folder: " + currentPath);
            }
        }
        Debug.Log("Folder created at: " + path);
    }

    private void CreateFile(string path, string[] header)
    {
        if (!File.Exists(path))
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(";", header));
            }
        }

        Debug.Log("File created at: " + path);
    }

    private void AppendLine(string path, string[] data)
    {
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine(string.Join(";", data));
        }
    }
}
