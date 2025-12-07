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
    private List<int> freeSeatHistory = new List<int>();
    private const int maxPoints = 180;

    void Start()
    {
        floorRowTemplate.SetActive(false);
        buildingDropdown.onValueChanged.AddListener(OnBuildingChanged);
        StartCoroutine(FetchAPIRepeated());

        InitChart();   
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
    }

    void OnBuildingChanged(int index)
    {
        UpdateBuildingUI();
    }

    void UpdateBuildingUI()
    {
        if (latestData == null) return;

        string uiName = buildingDropdown.options[buildingDropdown.value].text;
        string targetName = uiName == "One Pool Street"
            ? "East Campus - Pool St"
            : "East Campus - Marshgate";

        // find data
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

        if (targetSurvey == null) return;

        int free = targetSurvey["sensors_absent"].Value<int>();
        int occ = targetSurvey["sensors_occupied"].Value<int>();
        int total = free + occ;

        float rate = total > 0 ? (float)free / total : 0f;

        // update UI
        totalFreeText.text = $"Free: {free}";
        totalOccupiedText.text = $"Occupied: {occ}";
        freeRateText.text = $"FreeRate: {Mathf.RoundToInt(rate * 100)}%";
        occupyBarFill.fillAmount = rate;

        UpdateFloorRows((JArray)targetSurvey["maps"]);

        freeSeatHistory.Add(free);
        if (freeSeatHistory.Count > maxPoints)
            freeSeatHistory.RemoveAt(0);

        UpdateTrendChart();
    }

    void UpdateFloorRows(JArray floors)
    {
        foreach (Transform c in floorContent)
            Destroy(c.gameObject);

        foreach (var f in floors)
        {
            GameObject row = Instantiate(floorRowTemplate, floorContent);
            row.SetActive(true);

            row.transform.Find("FloorName").GetComponent<TMP_Text>().text = f["name"].ToString();
            row.transform.Find("FreeSeats").GetComponent<TMP_Text>().text = $"Free: {f["sensors_absent"]}";
            row.transform.Find("OccupySeats").GetComponent<TMP_Text>().text = $"Occupied: {f["sensors_occupied"]}";
        }
    }


    void InitChart()
{
    if (trendChart == null) return;
    trendChart.ClearData();
}


    void UpdateTrendChart()
{
    if (trendChart == null) return;
    if (freeSeatHistory.Count == 0) return;

    int serieIndex = 0;  

    trendChart.ClearData();

    for (int i = 0; i < freeSeatHistory.Count; i++)
    {
        trendChart.AddData(serieIndex, i, freeSeatHistory[i]);
    }
    trendChart.RefreshChart();  
}

}
