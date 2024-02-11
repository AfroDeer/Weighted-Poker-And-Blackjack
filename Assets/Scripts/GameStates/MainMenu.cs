//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : GameState
{
    //Interface for UI to quit game.
    public void QuitGame()
    {
        GameInstance.instance.QuitGame();
    }

    //Interface for UI to open logs folder.
    public void OpenLogsFolder()
    {
        GameInstance.instance.OpenLogsFolder();
    }
}