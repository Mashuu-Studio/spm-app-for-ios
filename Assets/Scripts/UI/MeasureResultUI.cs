using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeasureResultUI : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> exerNames;
    [SerializeField] private List<TextMeshProUGUI> exerResults;

    public void SetResult(WebServerManager.MeasureResult result)
    {
        for (int i = 0; i < result.measurements.Length; i++)
        {
            var measure = result.measurements[i];

            exerNames[i].text = measure.name;

            string count = "";
            if (measure.unit == "PASS_FAIL")
            {
                if (measure.status == "SUCCESS") count = "성공";
                else if (measure.status == "FAIL") count = "실패";
            }
            else
            {
                var unit = "";
                if (measure.unit == "COUNT") unit = "회";
                else if (measure.unit == "SECONDS") unit = "초";
                count = $"{measure.value}{unit}";
            }
            exerResults[i].text = count;
        }
    }
}
