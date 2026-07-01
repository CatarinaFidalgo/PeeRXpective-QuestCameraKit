using UnityEngine;

/// <summary>
/// Capture 3 points on the table (A, B, C) in order,
/// then build a shared table-local coordinate frame.
/// Press one button three times while moving the pointer to A, then B, then C.
/// </summary>
public class Calibration : MonoBehaviour
{
    [Header("Status")]
    public bool isCalibrated = false;

    [Header("Input")]
    [Tooltip("The transform that is moved to touch table corner A, then B, then C.")]
    public Transform calibrationPointer;

    [Header("Capture Settings")]
    public float captureToleranceMeters = 0.05f;

    public enum CalibrationStep
    {
        None,
        A,
        B,
        C,
        Done
    }

    [Header("Runtime")]
    public CalibrationStep currentStep = CalibrationStep.None;

    [SerializeField] private Vector3 cornerAWorld;
    [SerializeField] private Vector3 cornerBWorld;
    [SerializeField] private Vector3 cornerCWorld;

    private bool _hasA;
    private bool _hasB;
    private bool _hasC;

    private Matrix4x4 _tableToWorld = Matrix4x4.identity;
    private Matrix4x4 _worldToTable = Matrix4x4.identity;
    private Quaternion _tableRotation = Quaternion.identity;

    private bool previousButtonState = false;

    private void Update()
    {
        bool currentButtonState = OVRInput.Get(OVRInput.RawButton.A);

        if (currentButtonState && !previousButtonState && !isCalibrated)
        {
            CaptureNextCorner();
        }

        previousButtonState = currentButtonState;
    }

    [ContextMenu("Reset Calibration")]
    public void ResetCalibration()
    {
        isCalibrated = false;
        currentStep = CalibrationStep.None;

        _hasA = false;
        _hasB = false;
        _hasC = false;

        cornerAWorld = Vector3.zero;
        cornerBWorld = Vector3.zero;
        cornerCWorld = Vector3.zero;

        _tableToWorld = Matrix4x4.identity;
        _worldToTable = Matrix4x4.identity;
        _tableRotation = Quaternion.identity;

        previousButtonState = false;
    }

    [ContextMenu("Capture Next Corner")]
    public void CaptureNextCorner()
    {
        if (!_hasA)
        {
            CaptureCornerA();
            return;
        }

        if (!_hasB)
        {
            CaptureCornerB();
            return;
        }

        if (!_hasC)
        {
            CaptureCornerC();
            return;
        }

        //Debug.Log("[Calibration] Already calibrated.");
        return;
    }

    [ContextMenu("Capture Corner A")]
    public void CaptureCornerA()
    {
        CaptureCorner(ref _hasA, ref cornerAWorld, CalibrationStep.A);
    }

    [ContextMenu("Capture Corner B")]
    public void CaptureCornerB()
    {
        CaptureCorner(ref _hasB, ref cornerBWorld, CalibrationStep.B);
    }

    [ContextMenu("Capture Corner C")]
    public void CaptureCornerC()
    {
        CaptureCorner(ref _hasC, ref cornerCWorld, CalibrationStep.C);
    }

    private void CaptureCorner(ref bool hasCorner, ref Vector3 cornerWorld, CalibrationStep step)
    {
        if (calibrationPointer == null)
        {
            Debug.LogError("[Calibration] calibrationPointer is not assigned.");
            return;
        }

        cornerWorld = calibrationPointer.position;
        hasCorner = true;
        currentStep = step;

        Debug.Log($"[Calibration] Captured {step} at {cornerWorld}");

        if (_hasA && _hasB && _hasC)
        {
            ComputeTableTransform();
        }
    }

    private void ComputeTableTransform()
    {
        Vector3 a = cornerAWorld;
        Vector3 b = cornerBWorld;
        Vector3 c = cornerCWorld;

        Vector3 xAxis = (b - a).normalized;
        Vector3 yAxis = Vector3.Cross((b - a), (c - a)).normalized;

        if (xAxis.sqrMagnitude < 1e-8f || yAxis.sqrMagnitude < 1e-8f)
        {
            Debug.LogError("[Calibration] Calibration points are degenerate. Check A, B, C.");
            isCalibrated = false;
            return;
        }

        Vector3 zAxis = Vector3.Cross(xAxis, yAxis).normalized;
        yAxis = Vector3.Cross(zAxis, xAxis).normalized; // re-orthogonalize

        _tableToWorld = Matrix4x4.identity;
        _tableToWorld.SetColumn(0, new Vector4(xAxis.x, xAxis.y, xAxis.z, 0f));
        _tableToWorld.SetColumn(1, new Vector4(yAxis.x, yAxis.y, yAxis.z, 0f));
        _tableToWorld.SetColumn(2, new Vector4(zAxis.x, zAxis.y, zAxis.z, 0f));
        _tableToWorld.SetColumn(3, new Vector4(a.x, a.y, a.z, 1f));

        _worldToTable = _tableToWorld.inverse;
        _tableRotation = Quaternion.LookRotation(zAxis, yAxis);

        isCalibrated = true;
        currentStep = CalibrationStep.Done;

        Debug.Log("[Calibration] Calibration complete.");
    }

    public bool IsPointerNear(Transform marker)
    {
        if (calibrationPointer == null || marker == null)
            return false;

        return Vector3.Distance(calibrationPointer.position, marker.position) <= captureToleranceMeters;
    }

    public Vector3 WorldToTablePosition(Vector3 worldPosition)
    {
        return _worldToTable.MultiplyPoint3x4(worldPosition);
    }

    public Quaternion WorldToTableRotation(Quaternion worldRotation)
    {
        return Quaternion.Inverse(_tableRotation) * worldRotation;
    }

    public Vector3 TableToWorldPosition(Vector3 tablePosition)
    {
        return _tableToWorld.MultiplyPoint3x4(tablePosition);
    }

    public Quaternion TableToWorldRotation(Quaternion tableRotation)
    {
        return _tableRotation * tableRotation;
    }

    public Matrix4x4 WorldToTableMatrix => _worldToTable;
    public Matrix4x4 TableToWorldMatrix => _tableToWorld;
    public Quaternion TableRotation => _tableRotation;

    public Vector3 CornerAWorld => cornerAWorld;
    public Vector3 CornerBWorld => cornerBWorld;
    public Vector3 CornerCWorld => cornerCWorld;
}