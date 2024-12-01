using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    [SerializeField] TMP_InputField idInputField;
    [SerializeField] TMP_InputField pwInputField;

    public string Id { get { return idInputField.text; } }
    public string Password { get { return pwInputField.text; } }
}
