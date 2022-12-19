using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class LoginAndRegisterBehaviour : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameField;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private GameObject blankInputMessage;
    [SerializeField]
    private GameObject invalidInputMessage;
    private string allowableChars = "^[a-zA-Z0-9_~`!@#$%^&*?]+$";

    private bool inputIsValid;
    private NetworkedClient client;
    private string inputMessage;

    [SerializeField]
    private Canvas gameScreen;


    private void Start()
    {
        client = FindObjectOfType<NetworkedClient>();
    }

    public void SwitchToGameScreen()
    {
        gameScreen.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void LoginClicked()
    {
        inputIsValid = CheckIfInputIsValid(0);

        if (inputIsValid == true)
        {
            AttemptLogin();
        }
    }

    private void AttemptLogin()
    {
        client.SendMessageToHost(inputMessage);
    }

    public void RegisterClicked()
    {
        inputIsValid = CheckIfInputIsValid(1);

        if (inputIsValid == true)
        {
            CreateAccount();
        }
    }

    private void CreateAccount()
    {
        client.SendMessageToHost(inputMessage);
    }

    private bool CheckIfInputIsValid(int EventType)
    {
        if (usernameField.text == "" || passwordField.text == "")
        {
            blankInputMessage.SetActive(true);
            return false;
        }
        else
        {
            //Check username for invalid characters
            Match usernameNotInvalid = Regex.Match(usernameField.text, "^[a-zA-Z0-9_~`!@#$%^&*?]+$");
            if (usernameField.text.Length < 6 && usernameNotInvalid.Success == false)
            {
                invalidInputMessage.SetActive(true);
                return false;
            }
            //Check password for invalid characters
            Match passwordNotInvalid = Regex.Match(passwordField.text, "^[a-zA-Z0-9_~`!@#$%^&*?]+$");
            if (passwordField.text.Length < 6 && passwordNotInvalid.Success == false)
            {
                return false;
            }
            inputMessage = EventType.ToString() + "," + usernameField.text + "," + passwordField.text;
            return true;
        }
    }


}
