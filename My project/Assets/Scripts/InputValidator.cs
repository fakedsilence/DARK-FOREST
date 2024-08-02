using System;
using TMPro;
using UnityEngine;

public class InputValidator : MonoBehaviour
{
    public TMP_InputField tmpInputField;
    public TextMeshProUGUI tmp;

    void Start()
    {
        if (tmpInputField != null)
        {
            Debug.Log("Listener started");
            tmpInputField.onValidateInput += ValidateInput;
        }
        else
        {
            Debug.Log("Input field not exist");
        }
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // 检查字符是否是 ASCII 范围内的字母或数字
        if (char.IsLetterOrDigit(addedChar) && addedChar < 128)
        {
            tmp.text = text + addedChar;
            return addedChar;
        }
        MenuView.Instance.ShowPopup("密码只能包含英文字母和数字");
        return '\0';  // 返回空字符，表示不接受这个字符
    }

    void OnDestroy()
    {
        if (tmpInputField != null)
        {
            tmpInputField.onValidateInput -= ValidateInput;
        }
    }
}