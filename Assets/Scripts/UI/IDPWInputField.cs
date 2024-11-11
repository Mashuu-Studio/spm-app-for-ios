using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class IDPWInputField : MonoBehaviour
{
    [SerializeField] private GameObject selelctedFrame;
    [SerializeField] private TextMeshProUGUI placeholder;
    [SerializeField] private string placeholderString;
    private TMP_InputField inputField;
    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        selelctedFrame.SetActive(false);
        inputField.onSelect.AddListener(str => selelctedFrame.SetActive(true));
        inputField.onSelect.AddListener(str => placeholder.text = "");
        
        inputField.onDeselect.AddListener(str => selelctedFrame.SetActive(false));
        inputField.onDeselect.AddListener(str => placeholder.text = placeholderString);
    }
}
