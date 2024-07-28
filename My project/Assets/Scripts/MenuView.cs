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

    public GameObject bgPanel;

    public GameObject tips;

    public GameObject mainMenu;

    public GameObject[] rankArr;
    // 单例实例
    private static MenuView instance;

    // 公有属性用于获取实例
    public static MenuView Instance
    {
        get
        {
            // 如果实例未被分配，尝试找到它
            if (instance == null)
            {
                instance = FindObjectOfType<MenuView>();

                // 检查场景中是否有多个实例
                if (FindObjectsOfType<MenuView>().Length > 1)
                {
                    Debug.LogError("场景中存在多个 MenuView 实例");
                    return instance;
                }

                // 如果找不到实例，创建一个新的对象并附加 MenuView 组件
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<MenuView>();
                    singletonObject.name = typeof(MenuView).ToString() + " (Singleton)";

                    // 确保这个游戏对象在场景切换时不会被摧毁
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return instance;
        }
    }

    // 确保该实例在场景加载时的唯一性
    private void Awake()
    {
        // 如果有一个实例存在且不是当前脚本实例，销毁当前对象以确保单例唯一性
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

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
        if (usernameString.Length > 1 && passwordString.Length > 5)
        {
            Task<bool> tsk = AccountManager.Instance.SendCreateAccount(usernameString, passwordString, usernameString);
            tsk.ContinueWith(t =>
            {
                if (t.Result)
                {
                    Debug.Log("Account creation success");
                }
                else
                {
                    Debug.LogError("Account creation failed");
                    MenuView.Instance.ShowPopup("用户名已被占用");
                }
            }).ContinueWith(t =>
            {
                // loginPanel.SetActive(false);
                // bgPanel.SetActive(false);
                // mainMenu.SetActive(true);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        else if(usernameString.Length <= 1)
        {
            MenuView.Instance.ShowPopup("用户名应当为至少2位字符");
        }
        else
        {
            MenuView.Instance.ShowPopup("密码应当为至少2位字符");
        }
        
    }

    public void LoginButton()
    {
        if (usernameString.Length > 1 && passwordString.Length > 1)
        {
            Task<bool> tsk = AccountManager.Instance.SendLogin(usernameString, passwordString);
            bool canLogin = false;
            tsk.ContinueWith(t =>
            {
                if (t.Result)
                {
                    Debug.Log("login success");
                    canLogin = true;
                }
                else
                {
                    Debug.LogError("login failed");
                }
            }).ContinueWith(t =>
            {
                if (canLogin)
                {
                    loginPanel.SetActive(false);
                    bgPanel.SetActive(false);
                    mainMenu.SetActive(true);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public void ShowPopup(string message)
    {
        tips.GetComponent<TextMeshProUGUI>().text = message;
        tips.SetActive(true);
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
