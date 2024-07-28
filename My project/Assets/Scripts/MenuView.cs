using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    public GameObject itemCell;

    public Transform content;

    public TextMeshProUGUI userName;

    public TextMeshProUGUI password;

    public GameObject loginPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RankButton()
    {

    }

    string usernameString
    {
        get
        {
            if (string.IsNullOrEmpty(userName.text))
            {
                return "";
            }
            return userName.text.Substring(0, userName.text.Length - 1);
        }
    }

    string passwordString
    {
        get
        {
            if (string.IsNullOrEmpty(password.text))
            {
                return "";
            }
            return password.text.Substring(0, password.text.Length - 1);
        }
    }

    public void RegisterButton()
    {
        if (usernameString.Length > 2 && passwordString.Length > 2)
        {
            Task tsk = AccountManager.Instance.SendCreateAccount(usernameString, passwordString, usernameString);
            tsk.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.LogError("Account creation failed");
                }
                else
                {
                    Debug.Log(0);
                }
            }).ContinueWith(t =>
            {
                loginPanel.SetActive(false);
                Debug.Log(1);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public void LoginButton()
    {
        if (usernameString.Length > 2 && passwordString.Length > 2)
        {
            Task tsk = AccountManager.Instance.SendLogin(usernameString, passwordString);
            tsk.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.LogError("login failed");
                }
                else
                {
                    Debug.Log(0);
                }
            }).ContinueWith(t =>
            {
                loginPanel.SetActive(false);
                Debug.Log(1);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public static void ShowPopup(string title, string message)
    {
        //to be completed
        return;
    }

    public void UpdateLeaderboard(string[] playerNames, int[] scores)
    {
        // 清空现有的排行榜
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 生成新的排行榜项
        for (int i = 0; i < playerNames.Length; i++)
        {
            GameObject newItem = Instantiate(itemCell);
            Text[] texts = newItem.GetComponentsInChildren<Text>();
            texts[0].text = playerNames[i]; // 玩家名
            texts[1].text = scores[i].ToString(); // 分数
        }

        // 更新content的height以适应所有的排行榜项
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, playerNames.Length * itemCell.GetComponent<RectTransform>().sizeDelta.y);
    }
}
