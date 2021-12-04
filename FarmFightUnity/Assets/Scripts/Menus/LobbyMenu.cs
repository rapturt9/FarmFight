using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class LobbyMenu : MonoBehaviour
{
    public TextMeshProUGUI maxBotsText;
    public TMP_InputField privateLobbyText;
    public TextMeshProUGUI privateLobbyPlaceholderText;

    public HelperPhoton helperPhoton;
    public GameObject joinableLobbyPrefab;
    public RectTransform joinableLobbyTop;

    private List<GameObject> joinableLobbyButtons = new List<GameObject>();
    List<string> randomLobbyNames = new List<string> { "Potato", "Carrot", "Wheat", "Eggplant" };

    public static LobbyMenu LBMenu;

    void Awake()
    {
        if (LBMenu == null)
        {
            LBMenu = this;
        }
        else if (LBMenu != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshJoinableLobbies();
    }

    public void PlayGame(bool isHosting)
    {
        print("Joining Game");
        SceneVariables.isHosting = isHosting;
        SceneVariables.cameThroughMenu = true;

        helperPhoton.StopClient();

        if (isHosting && SceneVariables.lobbyId == "") // Random lobby name
        {
            GetRandomLobbyName();
        }

        SceneManager.LoadScene("MainScene");
    }

    void GetRandomLobbyName()
    {
        // Up to 6 (arbitrary) times, try to get a new lobby name that isn't taken already
        int maxAttempts = 6;
        int attempts = 0;
        do
        {
            SceneVariables.lobbyId = randomLobbyNames[Random.Range(0, randomLobbyNames.Count)].ToUpper();
            attempts++;
        }
        while ((attempts < maxAttempts) && helperPhoton.availableRoomNames.Contains(SceneVariables.lobbyId));

        // If it's still taken, add a random 2-digit number to the end
        // This is acceptably unlikely to cause a collision
        if (helperPhoton.availableRoomNames.Contains(SceneVariables.lobbyId))
        {
            SceneVariables.lobbyId += Random.Range(10, 100).ToString();
        }
    }

    public void TryJoinPrivateGame()
    {
        helperPhoton.TryJoinPrivateGame();
    }

    public void JoinPrivateGameSuccess()
    {
        print($"Joining Private Game \"{SceneVariables.lobbyId}\"");
        PlayGame(false);
    }

    public void JoinPrivateGameFail()
    {
        privateLobbyText.text = "";
        privateLobbyPlaceholderText.text = "Not Found";
    }

    public void EditLobbyId(string lobbyId)
    {
        SceneVariables.lobbyId = lobbyId.ToUpper();
    }

    public void EditMaxBots(float maxBotsF)
    {
        int maxBots = (int)maxBotsF;
        SceneVariables.maxBots = maxBots;
        maxBotsText.text = maxBots.ToString();
    }

    public void EditPrivate(bool toggle)
    {
        SceneVariables.isPrivate = toggle;
    }

    public void RefreshJoinableLobbies()
    {
        // Destroys old buttons
        foreach (var button in joinableLobbyButtons)
        {
            Destroy(button);
        }

        // DEBUG
        //string[] testRoomNames = new string[] {"ABC", "DEF", "GHI", "123", "456", "678", "FarmFight", "Lobby"};
        //var testRooms = from name in testRoomNames select new Room(name, new RoomOptions());

        // Creates new buttons
        int i = 0;
        foreach (var room in helperPhoton.availableRooms)
        {
            GameObject go = Instantiate(joinableLobbyPrefab, joinableLobbyTop);
            go.GetComponent<JoinableLobby>().Init(room.Name, room.PlayerCount);
            joinableLobbyButtons.Add(go);

            i++;
        }
    }
}
