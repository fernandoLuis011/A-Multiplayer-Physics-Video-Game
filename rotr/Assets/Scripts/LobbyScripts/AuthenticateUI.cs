using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour {

    // Reference to the UI button the player clicks to enter name.
    [SerializeField] private Button authenticateButton;
    [SerializeField] private RectTransform changeNameButtonTransform;
    [SerializeField] private RectTransform returnButtonTransform;
    
    private void Awake() {
        // When the authenticate button is clicked:
        authenticateButton.onClick.AddListener(() => {
            // Grab the player name entered through the EditPlayerName.
            // Call the authentication logic from the LobbyManager using that name.
            LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());

            // Move buttons to new positions
            changeNameButtonTransform.anchoredPosition = new Vector2(-3, 285);  // Change the position of name button
            returnButtonTransform.anchoredPosition = new Vector2(-2, -256);      // Change the position of the return button


            // Hide this UI after authenticating (so we don't see the button anymore).
            Hide();
        });
    }
    
    private void Hide() {

        // Make the enter button not visible 
        gameObject.SetActive(false);
    }



}