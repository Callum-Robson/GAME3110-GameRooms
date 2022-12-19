using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class GameRoomBehaviour : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField roomNameField;

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

    [SerializeField]
    private GameObject waitingPanel;


    private void Start()
    {
        client = FindObjectOfType<NetworkedClient>();
    }

    public void SwitchToGameScreen()
    {
        gameScreen.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        waitingPanel.SetActive(false);
    }

    public void ConnectButtonClicked()
    {
        inputIsValid = CheckIfInputIsValid(0);

        if (inputIsValid == true)
        {
            JoinRoom();
        }
    }

    private void JoinRoom()
    {
        inputIsValid = CheckIfInputIsValid(2);
        client.SendMessageToHost(inputMessage);
    }

    private bool CheckIfInputIsValid(int EventType)
    {
        if (roomNameField.text == "")
        {
            blankInputMessage.SetActive(true);
            return false;
        }
        else
        {
            //Check username for invalid characters
            Match usernameNotInvalid = Regex.Match(roomNameField.text, "^[a-zA-Z0-9_~`!@#$%^&*?]+$");
            if (roomNameField.text.Length < 6 && usernameNotInvalid.Success == false)
            {
                invalidInputMessage.SetActive(true);
                return false;
            }
            inputMessage = EventType + "," + roomNameField.text;
            return true;
        }
    }


}