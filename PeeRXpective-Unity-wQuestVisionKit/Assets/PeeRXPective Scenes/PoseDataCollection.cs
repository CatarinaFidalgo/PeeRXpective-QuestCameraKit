using System;
using System.Collections; // Added for Coroutine
using System.IO;
using System.Globalization;
using System.Text;
using UnityEngine;
using TMPro; // Added for TextMeshProUGUI
using SimpleWebRTC;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

[Serializable]
public class TrackedItem
{
    public string id;
    public Transform target;
}

/// <summary>
/// Logs one subject headset and screens in table-local coordinates.
/// Writes a metadata block first, then the CSV header, then the pose samples.
/// </summary>
public class PoseDataCollection : MonoBehaviour
{
    public static PoseDataCollection Instance;
    public DataCollection data;

    [Header("References")]
    public Calibration calibration;

    [Header("Tracked Subject")]
    [Tooltip("The one headset being logged in this file (current participant).")]
    public TrackedItem trackedSubject;

    [Header("Tracked Screens")]
    [Tooltip("Add one entry per screen you want to log.")]
    public TrackedItem[] trackedScreens;

    [Header("UI Labels Array")]
    [Tooltip("Drag the 4 TMP Labels here in order")]
    public TextMeshProUGUI[] screenLabels; // Added array for watching the UI

    [Header("Logging")]
    public bool autoStart = true;
    public float sampleRate = 30f;

    private string folderPath;
    private string filePath;
    private string participantID;

    private StreamWriter writer;
    private bool isRecording;
    private float nextSampleTime;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private WebRTCConnection webRTC;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif

        participantID = data.role.ToString() + "_pid" + data.pID;

        // Fill in Tracked Item IDs 
        webRTC = FindAnyObjectByType<WebRTCConnection>();
        trackedSubject.id = participantID;
        trackedScreens[0].id = webRTC.PreassignedLabelSlots[0].ToString();
        trackedScreens[1].id = webRTC.PreassignedLabelSlots[1].ToString();
        trackedScreens[2].id = webRTC.PreassignedLabelSlots[2].ToString();
        trackedScreens[3].id = webRTC.PreassignedLabelSlots[3].ToString();

        // Launch the watcher to update IDs when WebRTC changes the UI
        StartCoroutine(UpdateIdsOnConnection());

#if UNITY_ANDROID && !UNITY_EDITOR
        folderPath = Path.Combine("/sdcard/Documents", "DataCollection");
#else
        folderPath = Path.Combine(Application.persistentDataPath, "DataCollection");
#endif

        CreateFolder(folderPath);

        string timestamp = DateTime.Now.ToString("MM_dd_HH_mm");
        filePath = Path.Combine(folderPath, $"{timestamp}__{participantID}__PoseLogs.csv");

        if (autoStart)
        {
            // If calibration is already done, recording will start.
            // If not, Update() will start it as soon as isCalibrated becomes true.
            TryBeginRecording();
        }
    }

    void Update()
    {

        if (!autoStart)
            return;

        if (!isRecording && calibration != null && calibration.isCalibrated)
        {
            TryBeginRecording();
        }

        if (!isRecording)
            return;

        if (calibration == null || !calibration.isCalibrated)
            return;

        if (Time.time < nextSampleTime)
            return;

        nextSampleTime = Time.time + (1f / Mathf.Max(1f, sampleRate));
        WriteSampleBlock();
    }

    void OnApplicationQuit()
    {
        EndRecording();
    }

    void OnDestroy()
    {
        EndRecording();
    }

    // --- Added Coroutine for One-and-Done label updates ---
    private IEnumerator UpdateIdsOnConnection()
    {
        if (trackedScreens == null || screenLabels == null) yield break;

        int length = Mathf.Min(trackedScreens.Length, screenLabels.Length);
        string[] baselines = new string[length];
        bool[] isDone = new bool[length];
        int completedCount = 0;

        // 1. Record the baseline text for each label
        for (int i = 0; i < length; i++)
        {
            if (screenLabels[i] != null)
            {
                baselines[i] = screenLabels[i].text;
            }
        }

        // 2. Loop only until all screens have received a new name
        while (completedCount < length)
        {
            for (int i = 0; i < length; i++)
            {
                // If this screen hasn't been updated yet, check it
                if (!isDone[i] && screenLabels[i] != null)
                {
                    if (screenLabels[i].text != baselines[i])
                    {
                        // 3. Update your core variable!
                        trackedScreens[i].id = screenLabels[i].text;

                        // 4. Mark it as done so we never check it again
                        isDone[i] = true;
                        completedCount++;
                    }
                }
            }

            // Pause for half a second before checking again
            yield return new WaitForSeconds(0.5f);
        }
    }
    // ------------------------------------------------------

    public void TryBeginRecording()
    {
        if (isRecording)
            return;

        if (calibration == null || !calibration.isCalibrated)
        {
            Debug.LogWarning("[PoseDataCollection] Calibration is not complete yet.");
            return;
        }

        try
        {
            bool createNewFile = !File.Exists(filePath);

            writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.AutoFlush = true;

            if (createNewFile)
            {
                WriteMetadataBlock();
                WriteHeader();
                writer.Flush();
            }

            isRecording = true;
            nextSampleTime = Time.time;

            Debug.Log("[PoseDataCollection] Recording started: " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("[PoseDataCollection] Could not start recording: " + ex.Message);
            isRecording = false;
        }
    }

    public void EndRecording()
    {
        if (!isRecording && writer == null)
            return;

        isRecording = false;

        if (writer != null)
        {
            try
            {
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[PoseDataCollection] Error closing file: " + ex.Message);
            }
            finally
            {
                writer = null;
            }
        }

        Debug.Log("[PoseDataCollection] Recording stopped.");
    }

    private void WriteMetadataBlock()
    {
        writer.WriteLine("# PeerXpective pose log");
        writer.WriteLine("# ParticipantID=" + participantID);
        writer.WriteLine("# Role=" + data.role.ToString());
        writer.WriteLine("# CreatedAt=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        if (calibration != null)
        {
            writer.WriteLine("# CalibrationStatus=" + calibration.isCalibrated);
            writer.WriteLine("# CornerA=" + FormatVector(calibration.CornerAWorld));
            writer.WriteLine("# CornerB=" + FormatVector(calibration.CornerBWorld));
            writer.WriteLine("# CornerC=" + FormatVector(calibration.CornerCWorld));

            Matrix4x4 w2t = calibration.WorldToTableMatrix;
            writer.WriteLine("# WorldToTableMatrix=");
            writer.WriteLine("# " + FormatMatrixRow(w2t, 0));
            writer.WriteLine("# " + FormatMatrixRow(w2t, 1));
            writer.WriteLine("# " + FormatMatrixRow(w2t, 2));
            writer.WriteLine("# " + FormatMatrixRow(w2t, 3));
        }
    }

    private void WriteHeader()
    {
        writer.WriteLine(string.Join(";", new[]
        {
            "TimeStamp",
            "ParticipantID",
            "ObjectName",
            "ObjectType",
            "PosX",
            "PosY",
            "PosZ",
            "RotX",
            "RotY",
            "RotZ",
            "RotW"
        }));
    }

    private void WriteSampleBlock()
    {
        if (trackedSubject != null && trackedSubject.target != null)
        {
            string objectName = string.IsNullOrWhiteSpace(trackedSubject.id) ? "subject_headset" : trackedSubject.id;
            WriteTrackedTransform(objectName, "headset", trackedSubject.target);
        }

        if (trackedScreens != null)
        {
            for (int i = 0; i < trackedScreens.Length; i++)
            {
                if (trackedScreens[i] == null || trackedScreens[i].target == null)
                    continue;

                string objectName = string.IsNullOrWhiteSpace(trackedScreens[i].id)
                    ? $"screen_{i + 1}"
                    : trackedScreens[i].id;

                WriteTrackedTransform(objectName, "screen", trackedScreens[i].target);
            }
        }
    }

    private void WriteTrackedTransform(string objectName, string objectType, Transform t)
    {
        if (calibration == null || !calibration.isCalibrated)
            return;

        Vector3 localPos = calibration.WorldToTablePosition(t.position);
        Quaternion localRot = calibration.WorldToTableRotation(t.rotation);

        WriteRow(objectName, objectType, localPos, localRot);
    }

    private void WriteRow(string objectName, string objectType, Vector3 pos, Quaternion rot)
    {
        string[] data =
        {
            DateTime.Now.ToString("HH:mm:ss.fff"),
            participantID,
            objectName,
            objectType,
            pos.x.ToString("F6", Invariant),
            pos.y.ToString("F6", Invariant),
            pos.z.ToString("F6", Invariant),
            rot.x.ToString("F6", Invariant),
            rot.y.ToString("F6", Invariant),
            rot.z.ToString("F6", Invariant),
            rot.w.ToString("F6", Invariant)
        };

        AppendLine(filePath, data);
    }

    private void CreateFolder(string path)
    {
        var folders = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        string currentPath = "";

#if UNITY_ANDROID && !UNITY_EDITOR
        currentPath = "/";
#else
        if (folders.Length > 0 && folders[0].Contains(":"))
        {
            currentPath = folders[0] + "\\";
            string[] remainingFolders = new string[folders.Length - 1];
            Array.Copy(folders, 1, remainingFolders, 0, folders.Length - 1);
            folders = remainingFolders;
        }
        else
        {
            currentPath = "/";
        }
#endif

        foreach (var folder in folders)
        {
            currentPath = Path.Combine(currentPath, folder);
            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }
        }

        Debug.Log("[PoseDataCollection] Folder created at: " + path);
    }

    private void AppendLine(string path, string[] data)
    {
        using (StreamWriter fileWriter = new StreamWriter(path, true, Encoding.UTF8))
        {
            fileWriter.WriteLine(string.Join(";", data));
        }
    }

    private string FormatVector(Vector3 v)
    {
        return v.x.ToString("F6", Invariant) + "," +
               v.y.ToString("F6", Invariant) + "," +
               v.z.ToString("F6", Invariant);
    }

    private string FormatMatrixRow(Matrix4x4 m, int row)
    {
        return m[row, 0].ToString("F6", Invariant) + "," +
               m[row, 1].ToString("F6", Invariant) + "," +
               m[row, 2].ToString("F6", Invariant) + "," +
               m[row, 3].ToString("F6", Invariant);
    }

    public string GetFilePath()
    {
        return filePath;
    }
}

/*using System;
using System.IO;
using System.Globalization;
using System.Text;
using UnityEngine;
using SimpleWebRTC;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

[Serializable]
public class TrackedItem
{
    public string id;
    public Transform target;
}

/// <summary>
/// Logs one subject headset and screens in table-local coordinates.
/// Writes a metadata block first, then the CSV header, then the pose samples.
/// </summary>
public class PoseDataCollection : MonoBehaviour
{
    public static PoseDataCollection Instance;
    public DataCollection data;

    [Header("References")]
    public Calibration calibration;

    [Header("Tracked Subject")]
    [Tooltip("The one headset being logged in this file (current participant).")]
    public TrackedItem trackedSubject;

    [Header("Tracked Screens")]
    [Tooltip("Add one entry per screen you want to log.")]
    public TrackedItem[] trackedScreens;

    [Header("Logging")]
    public bool autoStart = true;
    public float sampleRate = 30f;

    private string folderPath;
    private string filePath;
    private string participantID;

    private StreamWriter writer;
    private bool isRecording;
    private float nextSampleTime;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private WebRTCConnection webRTC;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
                
        participantID = data.role.ToString() + "_pid" + data.pID;

        // Fill in Tracked Item IDs 
        webRTC = FindAnyObjectByType<WebRTCConnection>();
        trackedSubject.id = participantID;
        trackedScreens[0].id = webRTC.PreassignedLabelSlots[0].ToString();
        trackedScreens[1].id = webRTC.PreassignedLabelSlots[1].ToString();
        trackedScreens[2].id = webRTC.PreassignedLabelSlots[2].ToString();
        trackedScreens[3].id = webRTC.PreassignedLabelSlots[3].ToString();

#if UNITY_ANDROID && !UNITY_EDITOR
        folderPath = Path.Combine("/sdcard/Documents", "DataCollection");
#else
        folderPath = Path.Combine(Application.persistentDataPath, "DataCollection");
#endif

        CreateFolder(folderPath);

        string timestamp = DateTime.Now.ToString("MM_dd_HH_mm");
        filePath = Path.Combine(folderPath, $"{timestamp}__{participantID}__PoseLogs.csv");

        if (autoStart)
        {
            // If calibration is already done, recording will start.
            // If not, Update() will start it as soon as isCalibrated becomes true.
            TryBeginRecording();
        }
    }

    void Update()
    {
        
        if (!autoStart)
            return;

        if (!isRecording && calibration != null && calibration.isCalibrated)
        {
            TryBeginRecording();
        }

        if (!isRecording)
            return;

        if (calibration == null || !calibration.isCalibrated)
            return;

        if (Time.time < nextSampleTime)
            return;

        nextSampleTime = Time.time + (1f / Mathf.Max(1f, sampleRate));
        WriteSampleBlock();
    }

    void OnApplicationQuit()
    {
        EndRecording();
    }

    void OnDestroy()
    {
        EndRecording();
    }

    public void TryBeginRecording()
    {
        if (isRecording)
            return;

        if (calibration == null || !calibration.isCalibrated)
        {
            Debug.LogWarning("[PoseDataCollection] Calibration is not complete yet.");
            return;
        }

        try
        {
            bool createNewFile = !File.Exists(filePath);

            writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.AutoFlush = true;

            if (createNewFile)
            {
                WriteMetadataBlock();
                WriteHeader();
                writer.Flush();
            }

            isRecording = true;
            nextSampleTime = Time.time;

            Debug.Log("[PoseDataCollection] Recording started: " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("[PoseDataCollection] Could not start recording: " + ex.Message);
            isRecording = false;
        }
    }

    public void EndRecording()
    {
        if (!isRecording && writer == null)
            return;

        isRecording = false;

        if (writer != null)
        {
            try
            {
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[PoseDataCollection] Error closing file: " + ex.Message);
            }
            finally
            {
                writer = null;
            }
        }

        Debug.Log("[PoseDataCollection] Recording stopped.");
    }

    private void WriteMetadataBlock()
    {
        writer.WriteLine("# PeerXpective pose log");
        writer.WriteLine("# ParticipantID=" + participantID);
        writer.WriteLine("# Role=" + data.role.ToString());
        writer.WriteLine("# CreatedAt=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        if (calibration != null)
        {
            writer.WriteLine("# CalibrationStatus=" + calibration.isCalibrated);
            writer.WriteLine("# CornerA=" + FormatVector(calibration.CornerAWorld));
            writer.WriteLine("# CornerB=" + FormatVector(calibration.CornerBWorld));
            writer.WriteLine("# CornerC=" + FormatVector(calibration.CornerCWorld));

            Matrix4x4 w2t = calibration.WorldToTableMatrix;
            writer.WriteLine("# WorldToTableMatrix=");
            writer.WriteLine("# " + FormatMatrixRow(w2t, 0));
            writer.WriteLine("# " + FormatMatrixRow(w2t, 1));
            writer.WriteLine("# " + FormatMatrixRow(w2t, 2));
            writer.WriteLine("# " + FormatMatrixRow(w2t, 3));
        }
    }

    private void WriteHeader()
    {
        writer.WriteLine(string.Join(";", new[]
        {
            "TimeStamp",
            "ParticipantID",
            "ObjectName",
            "ObjectType",
            "PosX",
            "PosY",
            "PosZ",
            "RotX",
            "RotY",
            "RotZ",
            "RotW"
        }));
    }

    private void WriteSampleBlock()
    {
        if (trackedSubject != null && trackedSubject.target != null)
        {
            string objectName = string.IsNullOrWhiteSpace(trackedSubject.id) ? "subject_headset" : trackedSubject.id;
            WriteTrackedTransform(objectName, "headset", trackedSubject.target);
        }

        if (trackedScreens != null)
        {
            for (int i = 0; i < trackedScreens.Length; i++)
            {
                if (trackedScreens[i] == null || trackedScreens[i].target == null)
                    continue;

                string objectName = string.IsNullOrWhiteSpace(trackedScreens[i].id)
                    ? $"screen_{i + 1}"
                    : trackedScreens[i].id;

                WriteTrackedTransform(objectName, "screen", trackedScreens[i].target);
            }
        }
    }

    private void WriteTrackedTransform(string objectName, string objectType, Transform t)
    {
        if (calibration == null || !calibration.isCalibrated)
            return;

        Vector3 localPos = calibration.WorldToTablePosition(t.position);
        Quaternion localRot = calibration.WorldToTableRotation(t.rotation);

        WriteRow(objectName, objectType, localPos, localRot);
    }

    private void WriteRow(string objectName, string objectType, Vector3 pos, Quaternion rot)
    {
        string[] data =
        {
            DateTime.Now.ToString("HH:mm:ss.fff"),
            participantID,
            objectName,
            objectType,
            pos.x.ToString("F6", Invariant),
            pos.y.ToString("F6", Invariant),
            pos.z.ToString("F6", Invariant),
            rot.x.ToString("F6", Invariant),
            rot.y.ToString("F6", Invariant),
            rot.z.ToString("F6", Invariant),
            rot.w.ToString("F6", Invariant)
        };

        AppendLine(filePath, data);
    }

    private void CreateFolder(string path)
    {
        var folders = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        string currentPath = "";

#if UNITY_ANDROID && !UNITY_EDITOR
        currentPath = "/";
#else
        if (folders.Length > 0 && folders[0].Contains(":"))
        {
            currentPath = folders[0] + "\\";
            string[] remainingFolders = new string[folders.Length - 1];
            Array.Copy(folders, 1, remainingFolders, 0, folders.Length - 1);
            folders = remainingFolders;
        }
        else
        {
            currentPath = "/";
        }
#endif

        foreach (var folder in folders)
        {
            currentPath = Path.Combine(currentPath, folder);
            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }
        }

        Debug.Log("[PoseDataCollection] Folder created at: " + path);
    }

    private void AppendLine(string path, string[] data)
    {
        using (StreamWriter fileWriter = new StreamWriter(path, true, Encoding.UTF8))
        {
            fileWriter.WriteLine(string.Join(";", data));
        }
    }

    private string FormatVector(Vector3 v)
    {
        return v.x.ToString("F6", Invariant) + "," +
               v.y.ToString("F6", Invariant) + "," +
               v.z.ToString("F6", Invariant);
    }

    private string FormatMatrixRow(Matrix4x4 m, int row)
    {
        return m[row, 0].ToString("F6", Invariant) + "," +
               m[row, 1].ToString("F6", Invariant) + "," +
               m[row, 2].ToString("F6", Invariant) + "," +
               m[row, 3].ToString("F6", Invariant);
    }

    public string GetFilePath()
    {
        return filePath;
    }
}*/