using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;
using System.IO;

public class NetworkedServer : MonoBehaviour
{
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;

    const string zero = "0";
    const string one = "1";
    const string two = "2";

    [SerializeField]
    private GameRoom gameRoomPrefab;
    [SerializeField]
    private List<GameRoom> activeGameRooms = new List<GameRoom>();

    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);
        
    }

    // Update is called once per frame
    void Update()
    {

        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0;

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                break;
        }

    }
  
    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    
    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
        string[] message = msg.Split(",");
        if (message[0] == zero)
        {
            TryLogin(message[1], message[2], id);
        }
        else if (message[0] == one)
        {
            TryCreateAccount(message[1], message[2], id);
        }
        else if (message[0] == two)
        {
            TryJoinGameRoom(message[1], id);
        }
    }

    private int TryJoinGameRoom(string roomName, int id)
    {
        Debug.Log("Trying to join game room");
        bool roomFound = false;
        string fileName = "GameRoom_" + roomName + ".txt";
        if (File.Exists(fileName)) //"GameRoom_" + roomName + ".txt"))
        {
            using (StreamReader sr = new StreamReader(fileName))   //"GameRoom_" + roomName + ".txt"))
            {
                string line;
                string[] csv = new string[] { };

                while ((line = sr.ReadLine()) != null && !roomFound)
                {
                    csv = line.Split(",");
                    roomFound = true;
                    if (activeGameRooms.Count > 0)
                    {
                        if (activeGameRooms[int.Parse(csv[0])].CheckIfRoomIsFull() == false)
                        {
                            activeGameRooms[int.Parse(csv[0])].AddPlayerToRoom(id);
                            SendMessageToClient("Joined Game Room", id);
                        }
                    }
                }
                //sr.Close();
            }
        }
        else
        {
            CreateGameRoom(roomName, id);
        }
        return 0;
    }

    private int CreateGameRoom(string roomName, int id)
    {
        Debug.Log("Room doesn't exist, creating Game Room");
        // Save Game Room info to database
        using (StreamWriter sw = new StreamWriter("GameRoom_" + roomName + ".txt"))
        {
            sw.WriteLine(activeGameRooms.Count + "," + roomName);
            sw.Close();
        }
        GameRoom newGameRoom = Instantiate(gameRoomPrefab, null);
        newGameRoom.name = roomName;
        newGameRoom.InitializeRoom(roomName, id);
        activeGameRooms.Add(newGameRoom);
        SendMessageToClient("Joined Game Room", id);
        return 0;
    }

    public int DestroyGameRoom(string roomName)
    {
        string fileName = "GameRoom_" + roomName + ".txt";

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
        return 0;
    }

    private int TryLogin(string username, string password, int id)
    {
        bool usernameFound = false;
        bool passwordMatchFound = false;

        // Check if username exists
        using (StreamReader sr = new StreamReader("AccountDatabase.txt"))
        {
            string line;
            string[] csv = new string[] { };

            while ((line = sr.ReadLine()) != null && !usernameFound)
            {
                csv = line.Split(",");

                for (int i = 0; i < csv.Length / 2; i++)    
                {
                    if (csv[i + (1 * i)] == username)   // Idk why I did this, can just use csv[0] as one set of login info is stored per line
                    {
                        usernameFound = true;
                    }
                }
            }
            // If username exists, check if received password matches saved password
            if (usernameFound == true)
            {
                for (int i = 1; i < (csv.Length / 2) + 1; i++)  
                {
                    // Idk why I did this, can just use csv[1] as one set of login info is stored per line
                    if (csv[i + (1 * (i - 1))] == password)  //1 + (1 * (1 -1) = 1; 2 + (1 * (2 - 1) = 3
                    {
                        sr.Close();
                        passwordMatchFound = true;
                        SendMessageToClient("Login success", id);
                        AssociateConnectionID(username, id);
                        return 0;
                    }
                }
                if (passwordMatchFound == false)
                {
                    sr.Close();
                    SendMessageToClient("Password incorrect", id);
                    return 0;
                }
            }
            // If username does not exist, send message to client 
            else
            {
                sr.Close();
                SendMessageToClient("No account with that username exists", id);
                return 0;
            }
            sr.Close();
            Debug.Log("login attempted");
        }
        return 0;
    }

    public void AssociateConnectionID(string username, int connectionID )
    {
        using (StreamWriter sw = new StreamWriter(username + "game" + ".txt"))
        {
            sw.WriteLine(connectionID + "," + username);
        }
    }
    public int TryCreateAccount(string username, string password, int id)
    {
        bool usernameFound = false;

        // Check if username exists, if so, send message to client that username is not available
        using (StreamReader sr = new StreamReader("AccountDatabase.txt"))
        {
            string line;
            string[] csv = new string[] { };

            while ((line = sr.ReadLine()) != null)
            {
                csv = line.Split(",");

                for (int i = 0; i < csv.Length / 2; i++)
                {
                    if (csv[i+(1*i)] == username)
                    {
                        sr.Close();
                        usernameFound = true;
                        SendMessageToClient("Username unavailable", id);
                        return 0;
                    }
                }
                //Don't need to check this here, maybe useful somewhere else

                //for (int i = 1; i < (csv.Length / 2) + 1; i++)
                //{
                //    if (csv[i + (1 * (i - 1))] == username)  //1 + (1 * (1 -1) = 1; 2 + (1 * (2 - 1) = 3
                //    {
                //        passwordFound = true;

                //        break;
                //    }
                //}
            }
                
        }

        if (usernameFound == false)
        {
            if (File.Exists("AccountDatabase.txt"))
            {
                FileStream fileAppend = File.Open("AccountDatabase.txt", FileMode.Append);
                // Else if username does not exist, save username and password to database
                using (StreamWriter sw = new StreamWriter(fileAppend))
                {
                    sw.WriteLine(username + "," + password);
                    sw.Close();
                }
                SendMessageToClient("Account Created", id);
            }
            else
            {
                // Else if username does not exist, save username and password to database
                using (StreamWriter sw = new StreamWriter("AccountDatabase.txt"))
                {
                    sw.WriteLine(username + "," + password);
                    sw.Close();
                    SendMessageToClient("Account Created", id);
                }
            }
        }
        return 0;
    }

}