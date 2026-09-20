using UnityEngine;

public class CalibrationLight : MonoBehaviour
{
    public Calibration cal;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cal.isCalibrated)
        {
            gameObject.SetActive(false);
            //gameObject.transform.color = Color.green;
        }
    }
}
