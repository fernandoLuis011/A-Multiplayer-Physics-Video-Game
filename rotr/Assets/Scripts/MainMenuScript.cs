using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed for Scene management (loading scenes)

public class MainMenuScript : MonoBehaviour
{
    // Method to load the game scene
    public void Play()
    {
        // Load the scene at index 1 (the game scene)
        SceneManager.LoadScene("LobbiesMenu"); 
    }

    // Method to quit the game
    public void QuitGame()
    {
        EditPlayerName.Instance.OnApplicationQuit();

        // Closes the application.
        Application.Quit();
    }
}
