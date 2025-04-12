using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InputWindow : MonoBehaviour {
    
    //Instance so the window can be accessed statically
    public static UI_InputWindow instance;

    // UI elements for the OK and Cancel buttons 
    private Button_UI okBtn;
    private Button_UI cancelBtn;

    // UI components for text display and input field
    private TextMeshProUGUI titleText;
    private TMP_InputField inputField;

    // References to hide these
    [SerializeField] private GameObject inputFieldGO;
    [SerializeField] private GameObject okBtnGO;
    [SerializeField] private GameObject cancelBtnGO;

    private void Awake() {
        instance = this;

        // Find button and text components by name in the hierarchy
        okBtn = transform.Find("okBtn").GetComponent<Button_UI>();
        cancelBtn = transform.Find("cancelBtn").GetComponent<Button_UI>();
        titleText = transform.Find("titleText").GetComponent<TextMeshProUGUI>();
        inputField = transform.Find("inputField").GetComponent<TMP_InputField>();

        Hide(); // Initially hide the window
    }

    //Allows keyboard shortcuts: 
    // - Enter submits
    // - Escape cancels
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            okBtn.ClickFunc();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            cancelBtn.ClickFunc();
        }
    }

    //Shows the input window and sets up its functionality.
    private void Show(string titleString, string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onOk) {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        titleText.text = titleString;

        //Set character limit and validation
        inputField.characterLimit = characterLimit;
        inputField.onValidateInput = (string text, int charIndex, char addedChar) => {
            return ValidateChar(validCharacters, addedChar);
        };

        inputField.text = inputString;
        inputField.Select();


        // Define what happens when OK is clicked
        okBtn.ClickFunc = () => {
            Hide();

            // When player clicks ok, takes the input and changes name
            onOk(inputField.text);
        };

        // Define what happens when Cancel is clicked
        cancelBtn.ClickFunc = () => {
            Hide();
            // Dont do anything
            onCancel();
        };
    }

    // Hides the popup window.
    private void Hide() {
        gameObject.SetActive(false);
    }

    // Validates whether the typed character is allowed.
    private char ValidateChar(string validCharacters, char addedChar) {
        if (validCharacters.IndexOf(addedChar) != -1) {
            // Valid
            return addedChar;
        } else {
            // Invalid
            return '\0';
        }
    }   

    // Static wrapper for showing the window (string input).
    // Accessible globally without referencing instance.
    public static void Show_Static(string titleString, string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onOk) {
        instance.Show(titleString, inputString, validCharacters, characterLimit, onCancel, onOk);
    }

    // Static wrapper for showing the window (integer input).
    //Converts result to int and runs callback.
    public static void Show_Static(string titleString, int defaultInt, Action onCancel, Action<int> onOk) {
        instance.Show(titleString, defaultInt.ToString(), "0123456789-", 20, onCancel, 
            (string inputText) => {
                // Try to Parse input string
                if (int.TryParse(inputText, out int _i)) {
                    onOk(_i);
                } else {
                    onOk(defaultInt); // fallback if parse fails
                }
            }
        );
    }

    public static void ShowMessage_Static(string message, float autoHideSeconds = 2f) {
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();

        // Change font size for showing a message
        instance.titleText.fontSize = 14;
        instance.titleText.text = message;

        // Make buttons and input field not visibles
        instance.inputFieldGO.SetActive(false);
        instance.okBtnGO.SetActive(false);
        instance.cancelBtnGO.SetActive(false);


        // Start a countdown of 2 seconds, after that the window will be hiden
        instance.StartCoroutine(instance.AutoHide(autoHideSeconds));

    }

    private IEnumerator AutoHide(float delay) {
        yield return new WaitForSeconds(delay);
        Hide();

        // Make buttons and input field visible again
        instance.inputFieldGO.SetActive(true);
        instance.okBtnGO.SetActive(true);
        instance.cancelBtnGO.SetActive(true);
        instance.titleText.fontSize = 20;
    }   




   
}
