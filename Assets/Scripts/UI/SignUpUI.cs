using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SignUpUI : MonoBehaviour
{
    [SerializeField] TMP_InputField idInputField;
    [SerializeField] TMP_InputField pwInputField;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_InputField genderText;
    [SerializeField] TMP_InputField birthdayInputField;
    [SerializeField] TMP_InputField organizationText;

    public string Id { get { return idInputField.text; } }
    public string Password { get { return pwInputField.text; } }
    public string Name { get { return nameInputField.text; } }
    public string Gender { get { return genderText.text == "³²ÀÚ" ? "MALE" : "FEMALE"; } }
    public string BirthDay { get { return birthdayInputField.text.Replace(".","-"); } }
    public int OrganizationId { get; private set; }

    public static Dictionary<int, int> monthday = new Dictionary<int, int>()
    {
        { 1, 31 },
        { 2, 29 },
        { 3, 31 },
        { 4, 30 },
        { 5, 31 },
        { 6, 30 },
        { 7, 31 },
        { 8, 31 },
        { 9, 30 },
        { 10, 31 },
        { 11, 30 },
        { 12, 31 }
    };

    public WebServerManager.SignUpInfo SignUpInfo
    {
        get
        {
            signUpInfo.userName = Id;
            signUpInfo.password = Password;
            signUpInfo.name = Name;
            signUpInfo.gender = Gender;
            signUpInfo.birth = BirthDay;
            signUpInfo.organizationId = OrganizationId;

            return signUpInfo;
        }
    }

    private WebServerManager.SignUpInfo signUpInfo = new WebServerManager.SignUpInfo();

    private void Update()
    {
        if (gameObject.activeSelf) birthdayInputField.MoveTextEnd(false);
    }

    public void SelectOrganization(string name, int id)
    {
        organizationText.text = name;
        OrganizationId = id;
    }

    public void SelectGender(string gender)
    {
        genderText.text = gender;
    }

    public void SetBirthdayFormat(string str)
    {
        string text = "";
        string[] ymd = str.Split(".");
        bool b = false;
        str = "";
        foreach (var s in ymd)
        {
            str += s;
        }

        if (str.Length >= 4)
        {
            var year = str.Substring(0, 4);
            text = year;
            if (str.Length >= 6)
            {
                var monthstr = str.Substring(4, 2);
                var month = int.Parse(monthstr);
                if (month > 12)
                {
                    month = 12;
                    monthstr = "12";
                }
                if (month == 0)
                {
                    month = 1;
                    monthstr = "01";
                }
                text += $".{monthstr}";
                if (str.Length >= 8)
                {
                    var daystr = str.Substring(6);
                    var day = int.Parse(daystr);
                    if (day > monthday[month]) daystr = monthday[month].ToString();
                    if (day == 0) daystr = "01";
                    text += $".{daystr}";
                }
                else if (str.Length == 7)
                {
                    text += $".{str.Substring(6, 1)}";
                }
            }
            else if (str.Length == 5)
            {
                text += $".{str.Substring(4, 1)}";
            }
        }
        else
        {
            text = str;
        }

        birthdayInputField.text = text;
    }
}
