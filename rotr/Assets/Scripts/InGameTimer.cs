using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameTimer : MonoBehaviour
{
    public TextMeshPro inGameTimeText; // Reference to the TextMeshPro component to display the timer on screen
    public float inGameTimeRemaining;  // The time remaining on the in-game timer (in seconds)
    public bool inGametimerIsRunning;  // A flag to check if the timer is still running

    private void Start()
    {
        // Starts the timer automatically when the game starts
        inGametimerIsRunning = true;
    }

    void Update()
    {
        // If the timer is running, update the remaining time
        if (inGametimerIsRunning)
        {
            // Check if there's still time remaining
            if (inGameTimeRemaining > 0)
            {
                // Decrease the remaining time by the time that has passed since the last frame
                inGameTimeRemaining -= Time.deltaTime;

                // Ensure the timer doesn't go below 0
                inGameTimeRemaining = Mathf.Clamp(inGameTimeRemaining, 0, Mathf.Infinity);

                // Update the display to show the new remaining time
                DisplayTimerGame(inGameTimeRemaining);
            }
            else
            {
                // If the timer reaches zero, output the message and stop the timer
                Debug.Log("Match timer has run out!");

                // Set the timer to zero and stop the timer from running
                inGameTimeRemaining = 0;
                inGametimerIsRunning = false;
            }
        }
    }

    // Helper method to display the remaining time on the UI
    void DisplayTimerGame(float timeToDisplay)
    {
        // Adjust the display time by adding 1 second to account for countdown starting at 1 second
        timeToDisplay += 1;

        // Calculate minutes and seconds from the remaining time
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  // Get the full minutes
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);  // Get the remaining seconds

        // Format the time in the "mm:ss" format and set the text for the timer
        inGameTimeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }
}