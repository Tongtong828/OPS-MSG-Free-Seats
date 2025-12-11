using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using XCharts.Runtime;


public class DashboardController : MonoBehaviour
{
    [Header("API Settings")]
    public string apiUrl = "https://uclapi.com/workspaces/sensors/summary?survey_filter=student";
    public string apiToken;

    [Header("Trend Chart")]
    public LineChart trendChart;

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

    // --- Each building has its own history ---
    private Dictionary<string, List<int>> history = new Dictionary<string, List<int>>()
    {
        { "One Pool Street", new List<int>() },
        { "Marshgate", new List<int>() }
    };

    private const int maxPoints = 180;  // 30 minutes (180 * 10 sec updates)

    void Start()
    {
        floorRowTemplate.SetActive(false);
        buildingDropdown.onValueChanged.AddListener(OnBuildingChanged);

        InitializeChart();

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

        string targetName =
            uiName == "One Pool Street" ? "East Campus - Pool St" : "East Campus - Marshgate";

        // Find survey
        JObject targetSurvey = null;
        foreach (var item in latestData["surveys"])
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

        // Read seat numbers
        int totalFree = targetSurvey["sensors_absent"].Value<int>();
        int totalOccupied = targetSurvey["sensors_occupied"].Value<int>();
        int total = totalFree + totalOccupied;

        float freeRate = total > 0 ? (float)totalFree / total : 0f;

        // Update UI
        totalFreeText.text = "Free: " + totalFree;
        totalOccupiedText.text = "Occupied: " + totalOccupied;
        freeRateText.text = Mathf.RoundToInt(freeRate * 100) + "%";
        occupyBarFill.fillAmount = freeRate;

        UpdateFloorRows((JArray)targetSurvey["maps"]);

        // --- Add real-time history for the correct building ---
        List<int> list = history[uiName];
        list.Add(totalFree);
        if (list.Count > maxPoints)
            list.RemoveAt(0);

        UpdateTrendChart();
    }

    void UpdateFloorRows(JArray floors)
    {
        foreach (Transform child in floorContent)
            Destroy(child.gameObject);

        foreach (var f in floors)
        {
            GameObject row = Instantiate(floorRowTemplate, floorContent);
            row.SetActive(true);

            row.transform.Find("FloorName").GetComponent<TMP_Text>().text = f["name"].ToString();
            row.transform.Find("FreeSeats").GetComponent<TMP_Text>().text =
                "Free: " + f["sensors_absent"].Value<int>();
            row.transform.Find("OccupySeats").GetComponent<TMP_Text>().text =
                "Occupied: " + f["sensors_occupied"].Value<int>();
        }
    }

    //   XCharts: Trend Line Setup
    void InitializeChart()
    {
        trendChart.ClearData();

        if (trendChart.series.Count == 0)
            trendChart.AddSerie<Line>("FreeSeats");

        trendChart.series[0].lineStyle.width = 3;
    }

    void UpdateTrendChart()
    {
        string uiName = buildingDropdown.options[buildingDropdown.value].text;
        List<int> list = history[uiName];

        Serie serie = trendChart.series[0];
        serie.ClearData();

        for (int i = 0; i < list.Count; i++)
        {
            serie.AddData(i, list[i]);
        }

        trendChart.RefreshChart();
    }

    public float GetFreeRate(string buildingName)
{
    if (latestData == null) return 0f;

    string targetName =
        buildingName == "One Pool Street" ? "East Campus - Pool St" :
        "East Campus - Marshgate";

    JObject targetSurvey = null;
    foreach (var item in latestData["surveys"])
    {
        if (item["name"].ToString() == targetName)
        {
            targetSurvey = (JObject)item;
            break;
        }
    }

    if (targetSurvey == null) return 0f;

    int free = targetSurvey["sensors_absent"].Value<int>();
    int occ = targetSurvey["sensors_occupied"].Value<int>();
    int total = free + occ;

    if (total == 0) return 0;
    return (float)free / total;
}

public void SetBuilding(string buildingName)
{
    int index = buildingDropdown.options.FindIndex(o => o.text == buildingName);
    if (index >= 0)
        buildingDropdown.value = index;
}
public string GetSelectedBuildingName()
{
    return buildingDropdown.options[buildingDropdown.value].text;
}


}
