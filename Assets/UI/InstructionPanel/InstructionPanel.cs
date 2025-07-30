using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class InstructionPanel : MonoBehaviour
{
    public string dynamicText = "Welcome to the game!";
    private VisualElement root;
    private bool dismissed = false;
    // public InputActionReference anyButton;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        Label introText = root.Q<Label>("introText");
        introText.text = dynamicText;
    }

    void Update()
    {
        if (!dismissed && Input.anyKeyDown)
        {
            root.style.display = DisplayStyle.None;
            dismissed = true;
            // Continue game logic here if needed
        }
    }
}
