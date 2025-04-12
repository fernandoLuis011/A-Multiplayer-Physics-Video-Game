using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
    private static NetworkBootstrap instance;

    private void Awake()
    {
        // Prevent duplicate NetworkManagers when returning to main menu
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist through scene loads
    }
}
