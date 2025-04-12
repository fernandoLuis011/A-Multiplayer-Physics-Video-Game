using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The SoundManager class handles the centralized management of sound effects in the game.
/// It follows the singleton pattern to ensure only one instance exists, allowing global access to play sounds.
/// </summary>
public class SoundManager : MonoBehaviour
{   
    /// <summary>
    /// Singleton instance of the SoundManager.
    /// This allows any script to access the SoundManager without creating multiple instances.
    /// </summary>
    public static SoundManager instance { get; private set; }

    /// <summary>
    /// Reference to the AudioSource component that plays sound effects.
    /// </summary>
    private AudioSource source;

    /// <summary>
    /// Unity's Awake method, called before the Start method.
    /// Initializes the singleton instance and gets the AudioSource component attached to this GameObject.
    /// </summary>
    private void Awake() {
        // Set the instance to this object, ensuring only one SoundManager exists
        instance = this;
        
        // Get the AudioSource component attached to this GameObject
        source = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a given sound effect once.
    /// </summary>
    /// <param name="_sound">The AudioClip to play.</param>
    public void PlaySound(AudioClip _sound){
        // Play the provided sound clip once at its default volume
        source.PlayOneShot(_sound); 
    }
}

