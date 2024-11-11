using TMPro;
using UnityEngine;

public class AlertUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    public void Alert(string title, string message)
    {
        titleText.text = $"<font-weight=\"700\">{title}";
        messageText.text = $"<font-weight=\"500\">{message}";
        gameObject.SetActive(true);
    }
}