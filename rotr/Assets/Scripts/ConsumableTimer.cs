using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class ConsumableTimer : MonoBehaviour
{
    public float totalTimeRemaining;             // Total time remaining for the consumable effect
    public float timeConsumableRemaining;        // Time remaining for the specific consumable boost
    public bool consumableTimerIsRunning;        // Whether the timer is currently running
    private const float threshold = 0.01f;       // A small threshold to prevent floating-point errors when time reaches 0

    private bool usesWASD;                      // Whether the player uses WASD (indicating boy character)

    // UI elements for displaying consumable timers and indicators for different players
    public TextMeshPro consumableTimeTextBoy;    // Timer text for the boy player
    public Image consumableIndicatorBoy;         // Consumable indicator for the boy player 
    public TextMeshPro consumableTimeTextGirl;   // Timer text for the girl player
    public Image consumableIndicatorGirl;        // Consumable indicator for the girl player 

    void Update()
    {
        // Check if the consumable timer is running
        if (consumableTimerIsRunning)
        {   
            // Display the timer and indicator based on the control scheme (WASD or another)
            if (usesWASD)
            {
                consumableTimeTextBoy.gameObject.SetActive(true); 
                consumableIndicatorBoy.gameObject.SetActive(true); 
            }
            else
            {
                consumableTimeTextGirl.gameObject.SetActive(true); 
                consumableIndicatorGirl.gameObject.SetActive(true);   
            }

            // If time remaining is above a small threshold, continue the countdown
            if (timeConsumableRemaining > threshold)
            {   
                timeConsumableRemaining -= Time.deltaTime; // Decrease time remaining based on the frame time (delta time)
                DisplayConsumableTime(timeConsumableRemaining);  // Update the UI with the new time remaining
                UpdateConsumableIndicator(timeConsumableRemaining); // Update the visual progress bar
            }
            else
            {
                // If time has run out, reset the timer and stop the consumable effect
                timeConsumableRemaining = 0; // Ensure time remaining is exactly 0
                consumableTimerIsRunning = false; // Stop the timer

                // Hide the UI elements when the consumable effect has ended
                if(usesWASD)
                {
                    consumableTimeTextBoy.gameObject.SetActive(false); // Hide the timer text
                    consumableIndicatorBoy.gameObject.SetActive(false); // Hide the indicator image
                }
                else
                {
                    consumableTimeTextGirl.gameObject.SetActive(false);  // Hide the timer text
                    consumableIndicatorGirl.gameObject.SetActive(false);  // Hide the indicator image
                }
            }
        } 
    }

    // Method to update the timer display based on the time remaining
    void DisplayConsumableTime(float timeToDisplay)
    {
        // Update the respective player's timer UI (depending on the control scheme)
        if(usesWASD)
        {
            consumableTimeTextBoy.text = string.Format("{0:0}", timeToDisplay);  // Format the time to show 1 decimal place
        }
        else
        {
            consumableTimeTextGirl.text = string.Format("{0:0}", timeToDisplay); // Format the time to show 1 decimal place
        }
    }

    // Method to update the consumable indicator
    void UpdateConsumableIndicator(float timeRemaining)
    {
        // Calculate the fill amount for the indicator (how much of the bar should be filled)
        const float timerRef = 1.0f; // Reference value for full progress (1.0f corresponds to 100%)
        float fillAmount = timerRef / timeRemaining;  // Calculate the progress as a fraction of the remaining time
        if(usesWASD)
        {
            consumableIndicatorBoy.fillAmount = fillAmount;  // Update the image fill amount for the boy player
        }
        else
        {
            consumableIndicatorGirl.fillAmount = fillAmount;  // Update the image fill amount for the girl player
        }
    }

    // Method to start or update the consumable timer with new values
    public void TimerOnConsumable(bool timerIsRunning, float boostDuration, float totalTime, bool WASD)
    {
        usesWASD = WASD;                            // Set the control scheme (WASD or other)
        totalTimeRemaining = totalTime;             // Set the total time for the consumable effect
        consumableTimerIsRunning = timerIsRunning;  // Set whether the timer is running
        timeConsumableRemaining = boostDuration;    // Set the duration for this consumable effect
    }
}