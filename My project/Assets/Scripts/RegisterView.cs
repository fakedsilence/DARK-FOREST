using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterView : MonoBehaviour
{
    public TMP_InputField playerNameInput;

    public Button startButton;

    // public GameObject dialogueBox;  //DISPLAY OR HIDE
    // public TextMeshProUGUI dialogueText;

    // [TextArea(1, 3)]
    // public string[] dialogueLines;
    // [SerializeField] private int currentLine;

    // private bool isScrolling = false;  //DEFAULT VALUE IS FALSE
    // [SerializeField] private float textSpeed;

    // public static bool isFinished = false;

    private void Start()
    {
        //添加InputField的监听器
        playerNameInput.onEndEdit.AddListener(onEndEdit);
        // dialogueText.text = dialogueLines[currentLine];
    }

    private void onEndEdit(string playerName)
    {
        Debug.Log(1);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // private void Update()
    // {
    //     if (dialogueBox.activeInHierarchy)
    //     {
    //         if (Input.GetMouseButtonUp(0))
    //         {
    //             if (isScrolling == false)
    //             {
    //                 currentLine++;
    //                 if (currentLine < dialogueLines.Length)
    //                 {
    //                     //dialogueText.text = dialogueLines[currentLine];  //LINE BY LINE
    //                     StartCoroutine(ScrollingText());
    //                 }
    //                 else
    //                 {
    //                     dialogueBox.SetActive(false);  //BOX HIDE
    //                 }
    //             }
    //         }
    //     }
    // }

    // public void ShowDialogue(string[] _newLines)
    // {
    //     dialogueLines = _newLines;
    //     currentLine = 0;
    //     StartCoroutine(ScrollingText());
    //     dialogueBox.SetActive(true);
    // }

    // private IEnumerator ScrollingText()
    // {
    //     isScrolling = true;
    //     dialogueText.text = "";

    //     foreach (char letter in dialogueLines[currentLine].ToCharArray())
    //     {
    //         dialogueText.text += letter;  //SHOW EACH WORD
    //         yield return new WaitForSeconds(textSpeed);
    //     }
    //     isScrolling = false;
    // }
}