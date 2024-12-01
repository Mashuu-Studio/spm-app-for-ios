using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeasureLogUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI successText;
    [SerializeField] private Button measureButton;
    [SerializeField] private Button checkButton;

    private WebServerManager.MeasureResult measureResult;
    private int id;

    public async Task SetLog(WebServerManager.MeasureLog measureLog)
    {
        id = measureLog.id;
        var ymd = measureLog.date.Split("-");
        dateText.text = $"{ymd[0]}.{ymd[1]}.{ymd[2]}";

        measureResult = await WebServerManager.Instance.GetMeasureValues(id);
        if (measureResult != null)
        {
            if (measureResult.measurements.Length < 6)
            {
                successText.text = "미완료";
                checkButton.gameObject.SetActive(false);
                measureButton.gameObject.SetActive(true);
            }
            else
            {
                successText.text = "완료";
                checkButton.gameObject.SetActive(true);
                measureButton.gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ViewLog()
    {
        UIController.Instance.ViewLog(measureResult);
    }
}
