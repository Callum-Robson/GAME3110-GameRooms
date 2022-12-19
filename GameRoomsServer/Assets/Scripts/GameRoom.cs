using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoom : MonoBehaviour
{
    private int connectedPlayers = 0;
    private int playerOneID;
    private int playerTwoID;
    private List<int> connectedUserIDs = new List<int>();
    private string roomName;
    public int roomID;
    private NetworkedServer server;

    private void Start()
    {
        server = FindObjectOfType<NetworkedServer>();
    }

    public void InitializeRoom(string name, int firstPlayerID)
    {
        Debug.Log("Initializing Room");
        roomName = name;
        //AddPlayerToRoom(firstPlayerID);
        playerOneID = firstPlayerID;
        Debug.Log("Adding player with ID " + firstPlayerID + " to game room");
        connectedPlayers += 1;
    }

    public void RemovePlayerFromRoom(int playerID)
    {
        connectedPlayers -= 1;
        if (connectedPlayers == 0)
        {
            server.DestroyGameRoom(this.gameObject.name);
        }
        // Send message to client disconnecting from room
    }

    public void AddPlayerToRoom(int playerID)
    {
        Debug.Log("Adding player with ID " + playerID + " to game room");
        playerTwoID = playerID;
        connectedPlayers += 1;
        Debug.Log("Connected Players = " + connectedPlayers);
        if (connectedPlayers == 2)
        {
            server.SendMessageToClient("Game Room Full", playerOneID);
            server.SendMessageToClient("Game Room Full", playerTwoID);
        }
    }

    public bool CheckIfRoomIsFull()
    {
        if (connectedUserIDs.Count < 2)
            return false;
        else
            return true;
    }

}
