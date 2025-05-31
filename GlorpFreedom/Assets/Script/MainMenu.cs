using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Starts the game, loading the in-game scene, initializing the game state, and setting up the in-game UI.
    /// </summary>
    public void ButtonStartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGame"); // Load the in-game scene.

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor for gameplay.
        Cursor.visible = false;                  // Hide the cursor.
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit game!");
    }
}
