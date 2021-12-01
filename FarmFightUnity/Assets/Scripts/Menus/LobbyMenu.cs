using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    public TextMeshProUGUI maxBotsText;
    public TMP_InputField privateLobbyText;
    public TextMeshProUGUI privateLobbyPlaceholderText;

    public HelperPhoton helperPhoton;
    public GameObject joinableLobbyPrefab;
    public RectTransform joinableLobbyTop;

    private float joinableLobbySpacing;
    private List<GameObject> joinableLobbyButtons = new List<GameObject>();

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
        // Get padding for lobby buttons
        joinableLobbySpacing = joinableLobbyPrefab.GetComponent<RectTransform>().rect.height;

        RefreshJoinableLobbies();
    }

    public void PlayGame(bool isHosting)
    {
        print("Joining Game");
        SceneVariables.isHosting = isHosting;
        SceneVariables.cameThroughMenu = true;
        helperPhoton.StopClient();
        SceneManager.LoadScene("MainScene");
    }

    public void TryJoinPrivateGame()
    {
        foreach (var roomName in helperPhoton.availableRoomNames)
        {
            if (SceneVariables.lobbyId == roomName)
            {
                print($"Joining Private Game \"{roomName}\"");
                PlayGame(false);
                return;
            }
        }
        privateLobbyText.text = "";
        privateLobbyPlaceholderText.text = "Not Found";
    }

    public void EditLobbyId(string lobbyId)
    {
        SceneVariables.lobbyId = lobbyId;
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

        // Creates new buttons
        int i = 0;
        foreach (var roomName in helperPhoton.availableRoomNames)
        {
            GameObject go = Instantiate(joinableLobbyPrefab, joinableLobbyTop);
            go.transform.localPosition = Vector3.down * joinableLobbySpacing * i;
            go.GetComponent<JoinableLobby>().lobbyId = roomName;
            joinableLobbyButtons.Add(go);

            i++;
        }
    }
}
