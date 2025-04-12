using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GamePaused = false; // Static variable to track whether the game is paused
    public GameObject pauseMenuUI;         // Reference to the Pause Menu UI element

    // Update is called once per frame
    void Update()
    {
        // Check for the Escape key input to toggle pause/resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GamePaused)
            {
                ResumeGame(); // If the game is paused, resume the game
            }
            else
            {
                PauseGame(); // If the game is not paused, pause the game
            }
        }
    }

    // This method is called to resume the game (unpause)
    public void ResumeGame()
    {
        // Disable the pause menu UI
        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f; // Set the time scale back to 1 so the game resumes
        GamePaused = false; // Set GamePaused to false since the game is no longer paused
        //Debug.Log(GamePaused);
    }

    // This method is called to pause the game
    public void PauseGame()
    {
        // Enable the pause menu UI so the player can interact with it
        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f; // Set the time scale to 0, which pauses the game
        GamePaused = true; // Set GamePaused to true to indicate the game is paused
    }

    // This method is called when the player wants to quit the game
    public void QuitGame()
    {
        Time.timeScale = 1f; // Reset the time scale back to 1 to prevent issues with time flow when quitting
        GamePaused = false; // Set GamePaused to false as the game is no longer active

        // Shut down the network 
        if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsListening)
        {
            Debug.Log("Shutting down the Network Manager");
            Unity.Netcode.NetworkManager.Singleton.Shutdown();
        }
        
        // Load the scene at index 0 (main menu or main scene)
        SceneManager.LoadScene("LobbiesMenu");
    }

    // This method is called to restart the game (rematch)
    public void Rematch()
    {
        Time.timeScale = 1f; // Reset time scale back to 1 so the game runs at normal speed

        // Load the scene at index 1 (game scene or level)
        SceneManager.LoadScene(1);

        GamePaused = false; // Set GamePaused to false as the game is no longer paused
    }
}