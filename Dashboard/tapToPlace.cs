using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class tapToPlace : MonoBehaviour
{
    public GameObject dashboardPrefab;
    public GameObject gaugePrefab;

    private GameObject spawnedDashboard;
    private GameObject spawnedGauge;

    private ARRaycastManager _arRaycastManager;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool isTouching = false;
    public float timeThreshold = 0.5f;

    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    public bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            isTouching = true;
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        touchPosition = default;
        isTouching = false;
        timeThreshold = 0;
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
                // --- put Dashboard ---
                if (spawnedDashboard == null)
                {
                    spawnedDashboard = Instantiate(dashboardPrefab, hitPose.position, hitPose.rotation);
                    return;
                }

                // --- put Gauge ---
                if (spawnedGauge == null)
                {
                    spawnedGauge = Instantiate(gaugePrefab, hitPose.position, hitPose.rotation);
                    return;
                }
            }
        }
    }
}
