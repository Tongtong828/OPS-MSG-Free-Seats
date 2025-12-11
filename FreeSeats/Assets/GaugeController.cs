using UnityEngine;

public class GaugeController : MonoBehaviour
{
    public Transform pointer;

    public float minAngle = -70f;   // 0%
    public float maxAngle = 70f;    // 100%

    private DashboardController dashboard;

    void Start()
    {
        dashboard = FindObjectOfType<DashboardController>();
        InvokeRepeating(nameof(UpdateGauge), 1f, 1f);
    }

    void UpdateGauge()
    {
        if (dashboard == null || pointer == null)
            return;

        // float freeRate = dashboard.GetFreeRate("One Pool Street");
        string building = dashboard.GetSelectedBuildingName();
        float freeRate = dashboard.GetFreeRate(building);

        float angle = Mathf.Lerp(minAngle, maxAngle, freeRate);

        pointer.localRotation = Quaternion.Euler(0, angle, 0); 
    }
}
