using UnityEngine;

public class GaugeController : MonoBehaviour
{
    [Header("Pointer (Transform)")]
    public Transform pointer;

    [Header("Angles")]
    public float minAngle = -90f;   // 0%
    public float maxAngle = 90f;    //  100%

    private DashboardController dashboard;

    void Start()
    {
        dashboard = FindObjectOfType<DashboardController>();

        // Update pointer pos/s
        InvokeRepeating(nameof(UpdateGauge), 1f, 1f);
    }

    void UpdateGauge()
    {
        if (dashboard == null || pointer == null)
            return;

        // default One Pool Street
        float freeRate = dashboard.GetFreeRate("One Pool Street");

        float angle = Mathf.Lerp(minAngle, maxAngle, freeRate);

        pointer.localRotation = Quaternion.Euler(0, angle, 0);
    }
}
