using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class DashboardController : MonoBehaviour
{
    [Header("API Settings")]
    public string apiUrl = "https://uclapi.com/workspaces/sensors/summary?survey_filter=student";
    public string apiToken;

    [Header("UI References")]
    public TMP_Dropdown buildingDropdown;
    public TMP_Text totalFreeText;
    public TMP_Text totalOccupiedText;
    public TMP_Text freeRateText;
    public Image occupyBarFill;

    [Header("Floor UI")]
    public GameObject floorRowTemplate;
    public Transform floorContent;

    private JObject latestData;

    void Start()
    {
        floorRowTemplate.SetActive(false);
        buildingDropdown.onValueChanged.AddListener(OnBuildingChanged);
        StartCoroutine(FetchAPIRepeated());
    }

    IEnumerator FetchAPIRepeated()
    {
        while (true)
        {
            yield return FetchAPI();
            yield return new WaitForSeconds(10f);
        }
    }

    IEnumerator FetchAPI()
    {
        string fullUrl = apiUrl + "&token=" + apiToken;
        UnityWebRequest www = UnityWebRequest.Get(fullUrl);

        Debug.Log("Requesting: " + fullUrl);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            latestData = JObject.Parse(www.downloadHandler.text);
            UpdateBuildingUI();
        }
        else
        {
            Debug.Log("API ERROR: " + www.error);
        }
    }

    void OnBuildingChanged(int index)
    {
        UpdateBuildingUI();
    }

    void UpdateBuildingUI()
{
    if (latestData == null) return;

    string uiName = buildingDropdown.options[buildingDropdown.value].text;

    string targetName;
    if (uiName == "One Pool Street")
        targetName = "East Campus - Pool St";
    else
        targetName = "East Campus - Marshgate";

    JArray surveys = (JArray)latestData["surveys"];
    JObject targetSurvey = null;

    foreach (var item in surveys)
    {
        if (item["name"].ToString() == targetName)
        {
            targetSurvey = (JObject)item;
            break;
        }
    }

    if (targetSurvey == null)
    {
        Debug.Log("Building not found: " + targetName);
        return;
    }

    int totalFree = targetSurvey["sensors_absent"].Value<int>();
    int totalOccupied = targetSurvey["sensors_occupied"].Value<int>();
    int total = totalFree + totalOccupied;

    float freeRate = (total > 0) ? (float)totalFree / total : 0f;

    totalFreeText.text = "Free: " + totalFree;
    totalOccupiedText.text = "Occupied: " + totalOccupied;
    freeRateText.text = "FreeRate: " + Mathf.RoundToInt(freeRate * 100) + "%";
    occupyBarFill.fillAmount = freeRate;

    UpdateFloorRows((JArray)targetSurvey["maps"]);
}


    void UpdateFloorRows(JArray floors)
    {
        foreach (Transform child in floorContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var f in floors)
        {
            GameObject row = Instantiate(floorRowTemplate, floorContent);
            row.SetActive(true);

            TMP_Text nameText = row.transform.Find("FloorName").GetComponent<TMP_Text>();
            TMP_Text freeText = row.transform.Find("FreeSeats").GetComponent<TMP_Text>();
            TMP_Text occText = row.transform.Find("OccupySeats").GetComponent<TMP_Text>();

            nameText.text = f["name"].ToString();
            freeText.text = "Free: " + f["sensors_absent"].Value<int>();
            occText.text = "Occupied: " + f["sensors_occupied"].Value<int>();
        }
    }
}
