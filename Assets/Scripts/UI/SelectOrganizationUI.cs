using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectOrganizationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI organizationName;
    [SerializeField] private Button selectButton;

    public void Init(string name, Action action)
    {
        organizationName.text = name;
        selectButton.onClick.AddListener(() => action());
    }
}
