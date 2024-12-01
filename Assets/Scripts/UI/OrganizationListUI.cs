using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class OrganizationListUI : MonoBehaviour
{
    [SerializeField] private SelectOrganizationUI selectOrganizationUIPrefab;
    [SerializeField] private RectTransform organizationListParent;
    [SerializeField] private RectTransform alertRect;
    private List<SelectOrganizationUI> selectOrganizationUIList;
    private Dictionary<int, string> orgDictionary;

    public int SelectedOrganization { get { return selectedOrganization; } }
    private int selectedOrganization;

    public async void Init()
    {
        orgDictionary = new Dictionary<int, string>();

        selectOrganizationUIPrefab.gameObject.SetActive(false);

        var orgList = (await WebServerManager.Instance.GetOrganizations()).orgList;

        for (int i = 0; i < orgList.Length; i++)
        {
            var org = orgList[i];
            var ui = Instantiate(selectOrganizationUIPrefab);
            ui.Init(org.name, () => SelectOrganization(org.name, org.id));
            ui.transform.SetParent(organizationListParent);
            ui.gameObject.SetActive(true);

            orgDictionary.Add(org.id, org.name);
        }

        organizationListParent.sizeDelta = new Vector2(780, 78 * orgList.Length + 60 * (orgList.Length - 1));
        alertRect.sizeDelta = new Vector2(900, 132 + 141 + 102 + organizationListParent.sizeDelta.y);
    }

    public void SetActive(bool b)
    {
        gameObject.SetActive(b);
    }

    public void SelectOrganization(string name, int id)
    {
        UIController.Instance.SelectOrganization(name, id);
        selectedOrganization = id;
    }

    public string GetOrganizationName(int id)
    {
        if (orgDictionary.ContainsKey(id))
        {
            return orgDictionary[id];
        }
        else return "";
    }
}
