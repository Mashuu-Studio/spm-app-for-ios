using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInputField;
    [SerializeField] TMP_InputField pwInputField;

    public string Email { get { return emailInputField.text; } }
    public string Password { get { return pwInputField.text; } }
}
