using System;
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

    public GameObject rankPanel;

    public GameObject[] rankArr;

    private void Start()
    {
        loginPanel.SetActive(true);
        rankPanel.SetActive(false);
    }

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

    public async void showLeaderboard()
    {
        int uid = AccountManager.Instance.UserId;
        List<string> playerNames = new List<string>();
        List<int> scores = new List<int>();
        List<AccountManager.LeaderboardElement> element = await AccountManager.Instance.SendLeaderboardRequest(uid);
        if (element.Count == 0)
        {
            Debug.Log("空");
        }
        else
        {
            for (int i = 0; i < element.Count; i++)
            {
                playerNames.Add(element[i].name);
                scores.Add(element[i].score);
            }
            UpdateLeaderboard(playerNames, scores);
        }
    }

    public void UpdateLeaderboard(List<string> playerNames, List<int> scores)
    {
        rankPanel.SetActive(true);

        // 生成新的排行榜项
        for (int i = 0; i < rankArr.Length; i++)
        {
            rankArr[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            rankArr[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = playerNames[i];
            rankArr[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = scores[i].ToString();
        }
    }
}
