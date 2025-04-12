using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HeartCount : MonoBehaviour
{
    public Image[] lives;      // Array to hold the Image references for heart icons (representing player lives)
    public int livesRemaining; // Number of lives remaining for the player

    // Method to decrease the player's lives
    public void LoseLife()
    {
        // Decrease the number of lives remaining
        livesRemaining--;

        // Disable the current heart icon to indicate the loss of a life
        lives[livesRemaining].enabled = false;

        // Check if the player has no lives remaining
        if (livesRemaining == 0)
        {
            // Debug.Log("You Lost :()");
        }
    }

    // Method to add a heart to the player's lives
    public void AddHeart()
    {
        // Enable the next heart icon to show the player gained a life
        lives[livesRemaining].enabled = true;

        // Increase the number of remaining lives
        livesRemaining++;
    }
}