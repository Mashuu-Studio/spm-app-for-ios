using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeasureHistoryUI : MonoBehaviour
{
    [SerializeField] private MeasureLogUI logPrefab;
    private List<MeasureLogUI> measureLogUIs = new List<MeasureLogUI>();

    public async Task SetHistory()
    {
        logPrefab.gameObject.SetActive(false);
        var history = await WebServerManager.Instance.GetMeasureHistory();

        foreach (var lui in measureLogUIs)
        {
            Destroy(lui.gameObject);
        }
        measureLogUIs.Clear();

        foreach (var log in history.historyList)
        {
            var logui = Instantiate(logPrefab);
            measureLogUIs.Add(logui);
            await logui.SetLog(log);
            logui.transform.SetParent(transform);
            logui.gameObject.SetActive(true);
        }
    }
}
