using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class tapToPlace : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject dashboardPrefab;
    public GameObject gaugePrefab;

    private GameObject spawnedDashboard;
    private GameObject spawnedGauge;

    private ARRaycastManager _arRaycastManager;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public float timeThreshold = 0.5f;
    private bool isTouching = false;

    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    private bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            isTouching = true;
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        isTouching = false;
        timeThreshold = 0;
        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (isTouching)
            timeThreshold -= Time.deltaTime;

        if (!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if (_arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            if (timeThreshold < 0)
            {
                if (spawnedDashboard == null)
                {
                    spawnedDashboard = Instantiate(dashboardPrefab, hitPose.position, hitPose.rotation);
                    PlaceGaugeAutomatically();
                    return;
                }

                spawnedDashboard.transform.position = hitPose.position;

                // Follow Gauge 
                PlaceGaugeAutomatically();
            }
        }
    }

    private void PlaceGaugeAutomatically()
    {
        if (spawnedDashboard == null) return;

        Transform d = spawnedDashboard.transform;

        float offset = -1.1f;   
        Vector3 frontPos = d.position + d.forward * offset;

         Quaternion rot = d.rotation * Quaternion.Euler(0, 180f, 0); 

        if (spawnedGauge == null)
        {
            // First Generate Gauge
            spawnedGauge = Instantiate(gaugePrefab, frontPos, rot);
        }
        else
        {
            //Update Gauge Pos when Dashboard change Pos
            spawnedGauge.transform.position = frontPos;
            spawnedGauge.transform.rotation = rot;
        }
    }
}
