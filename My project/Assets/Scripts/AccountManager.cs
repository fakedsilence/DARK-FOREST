using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro.EditorUtilities;

public class AccountManager
{
    private static readonly Lazy<AccountManager> instance = new Lazy<AccountManager>(() => new AccountManager());
    private static HttpClient client;
    private int? _userId;
    private string _username;
    private string _password;
    private MenuView _menu;

    public MenuView Menu
    { get { return _menu; } }


    public int UserId
    {
        get
        {
            return _userId ?? 0;
        }
        set
        {
            _userId = value;
        }
    }

    public string Username
    {
        get
        {
            return _username;
        }
        set
        {
            _username = value;
        }
    }

    public string Password
    {
        get
        {
            return _password;
        }
        set
        {
            _password = value;
        }
    }

    // ˽�й��캯����������ⲿʵ��������
    private AccountManager()
    {
        InitializeClient();
    }

    public static AccountManager Instance
    {
        get
        {
            return instance.Value;
        }
    }

    public HttpClient Client
    {
        get
        {
            return client;
        }
    }

    private void InitializeClient()
    {
        client = new HttpClient();
        client.BaseAddress = new Uri("http://47.93.57.125:5000/");







    }

    public async Task<bool> SendLogin(string userName, string password)
    {
        return await LoginAsync(new
        {
            Username = userName,
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(password)), // base64 encoding
            LoginDate = DateTime.UtcNow
        });

    }

    public async Task<bool> SendCreateAccount(string userName, string password, string nickName)
    {

        return await CreateAccountAsync(new
        {
            Username = userName,
            Nickname = nickName,
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(password)), // base64 encoding
            RegistrationDate = DateTime.UtcNow
        });
        
    }

    public async Task<bool> SendScore(int uid, string password, int score)
    {
        return await SubmitScoreAsync(new
        {
            UserId = uid,
            SubmissionDate = DateTime.UtcNow,
            Score = score,
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(password))
        });
    }

    public async Task<bool> SendLeaderboardRequest(int uid)
    {
        return await RequestLeaderboard(uid);
    }

    private static async Task<bool> RequestLeaderboard(int uid)
    {

        // ��������ģ��
        var requestModel = new LeaderboardRequestModel
        {
            UID = uid
        };

        // ���л�����ģ��Ϊ JSON
        var json = JsonConvert.SerializeObject(requestModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("Scores/leaderboard", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            MenuView.ShowPopup("�޷���ȡ���а�", $"{response.StatusCode}: {errorMessage}");

            return false;
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<LeaderboardResponseModel>(responseString);

        // ��ӡ���������صĽ��
        List<string> names = new List<string>();
        List<int> scores = new List<int>();

        foreach (var player in responseObject.TopPlayers)
        {
            names.Add(player.Username);
            scores.Add(player.HighScore);
        }
        AccountManager.Instance.Menu.UpdateLeaderboard(names.ToArray(), scores.ToArray());
        return true;
    }


    private static async Task<bool> SubmitScoreAsync(object data)
    {
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("Scores/submit", content);
        response.EnsureSuccessStatusCode();
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            MenuView.ShowPopup("�޷��ύ����", $"{response.StatusCode}: {errorMessage}");

            return false;
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<RankResponse>(responseString);
        MenuView.ShowPopup("��Ϣ", $"�����ϴ��ɹ�!\n�µ�����: #{result.rank}");
        return true;
    }

    private static async Task<bool> CreateAccountAsync(object data)
    {
        Debug.Log("1");
        var json = JsonConvert.SerializeObject(data);
        Debug.Log("2");
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        Debug.Log("3");
        var response = await client.PostAsync("Scores/createAccount", content);
        Debug.Log("4");
        response.EnsureSuccessStatusCode();
        Debug.Log("6");
        if (!response.IsSuccessStatusCode)
        {
            Debug.Log("Failure");
            var errorMessage = await response.Content.ReadAsStringAsync();
            MenuView.ShowPopup("�޷������˺�", $"{response.StatusCode}: {errorMessage}");
            Debug.Log("�޷������˺�{response.StatusCode}: {errorMessage}");
            return false;
        }
        Debug.Log("Success");
        var responseString = await response.Content.ReadAsStringAsync();
        MenuView.ShowPopup("��Ϣ", "�����˺ųɹ�");
        Debug.Log("��Ϣ �����˺ųɹ�");
        return true;
    }

    private static async Task<bool> LoginAsync(object data)
    {
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("Scores/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.Log($"���Ե�½");
                return true;
                // ����ɹ��߼�
            }
            else
            {
                // ����ͬ�Ĵ���״̬��
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MenuView.ShowPopup("�޷���¼", $"{response.StatusCode}: {errorMessage}");
                    Debug.Log($"�޷���½{response.StatusCode}: {errorMessage}");

                    return false;
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            // ���������쳣�߼�
        }
        return true;
    }


    public class LeaderboardRequestModel
    {
        public int UID { get; set; }
    }

    public class LeaderboardEntry
    {
        public string Username { get; set; }
        public int HighScore { get; set; }
    }

    public class LeaderboardResponseModel
    {
        public List<LeaderboardEntry> TopPlayers { get; set; }
        public int PlayerRank { get; set; }
        public int PlayerHighScore { get; set; }
    }

    public class RankResponse
    {
        public string state { get; set; }
        public int rank { get; set; }
    }

}