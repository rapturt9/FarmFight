using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneVariables
{
    // DEBUG Did we click through the menu or go directly through the scene?
    public static bool cameThroughMenu = false;

    // Do we host our game?
    public static bool isHosting = false;

    // Maximum number of bots
    public static int maxBots = 0;

    // Lobby ID
    public static string lobbyId = "1";

    // Whether the lobby is private
    public static bool isPrivate = false;
}
