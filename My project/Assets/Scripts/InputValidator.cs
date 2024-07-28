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
        Debug.Log($"{text} {charIndex} {addedChar}");

        // ����ַ��Ƿ��� ASCII ��Χ�ڵ���ĸ������
        if (char.IsLetterOrDigit(addedChar) && addedChar < 128)
        {
            tmp.text = text + addedChar;
            return addedChar;
        }
        MenuView.Instance.ShowPopup("����ֻ�ܰ���Ӣ����ĸ������");
        return '\0';  // ���ؿ��ַ�����ʾ����������ַ�
    }

    void OnDestroy()
    {
        if (tmpInputField != null)
        {
            tmpInputField.onValidateInput -= ValidateInput;
        }
    }
}