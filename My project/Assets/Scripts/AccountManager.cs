using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.SocialPlatforms.Impl;

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

    // 私有构造函数，避免从外部实例化此类
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
        bool success = await LoginAsync(new
        {
            Username = userName,
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(password)), // base64 encoding
            LoginDate = DateTime.UtcNow
        });
        if (success)
        {
            AccountManager.Instance.Username = userName;
            AccountManager.Instance.Password = password;
        }
        return success;
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

    public class LeaderboardElement
    {
        public string name;
        public int score;
        public LeaderboardElement(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }


    public async Task<List<LeaderboardElement>> SendLeaderboardRequest(int uid)
    {
        return await RequestLeaderboard(uid);
    }

    private static async Task<List<LeaderboardElement>> RequestLeaderboard(int uid)
    {

        // 创建请求模型
        var requestModel = new LeaderboardRequestModel
        {
            UID = uid
        };

        // 序列化请求模型为 JSON
        var json = JsonConvert.SerializeObject(requestModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("Scores/leaderboard", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            MenuView.Instance.ShowPopup("无法获取排行榜");
            // +$"{response.StatusCode}: {errorMessage}"
            return new List<LeaderboardElement>();
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<LeaderboardResponseModel>(responseString);

        // 打印服务器返回的结果
        List<LeaderboardElement> result = new List<LeaderboardElement>();

        foreach (var player in responseObject.TopPlayers)
        {
            result.Add(new LeaderboardElement(player.Username, player.HighScore));
        }
        Debug.Log($"count: {result.Count}");
        return result;
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
            MenuView.Instance.ShowPopup("无法提交分数");
            //  + $"{response.StatusCode}: {errorMessage}"
            return false;
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<RankResponse>(responseString);
        MenuView.Instance.ShowPopup($"分数上传成功!\n新的排名: #{result.rank}");
        return true;
    }

    private static async Task<bool> CreateAccountAsync(object data)
    {

        var json = JsonConvert.SerializeObject(data);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("Scores/createAccount", content);



        if (!response.IsSuccessStatusCode)
        {
            Debug.Log("Failure");
            var errorMessage = await response.Content.ReadAsStringAsync();
            MenuView.Instance.ShowPopup("无法创建账号: ");
            //  + $"{response.StatusCode}: {errorMessage}"
            Debug.Log("无法创建账号{response.StatusCode}: {errorMessage}");
            return false;
        }
        Debug.Log("Success");
        var responseString = await response.Content.ReadAsStringAsync();
        MenuView.Instance.ShowPopup("创建账号成功");
        Debug.Log("信息 创建账号成功");
        return true;
    }

    private static async Task<bool> LoginAsync(object data)
    {
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        Debug.Log("wow");
        try
        {
            var response = await client.PostAsync("Scores/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.Log("here");
                Debug.Log(responseString);
                var responseObject = JsonConvert.DeserializeObject<LoginResponse>(responseString);
                string uid = responseObject?.Uid;
                AccountManager.Instance.UserId = int.Parse(uid);
                Debug.Log($"可以登陆, uid: {uid}");
                AccountManager.RequestLeaderboard(int.Parse(uid));
                return true;
                // 处理成功逻辑
            }
            else
            {
                // 处理不同的错误状态码


                var errorMessage = await response.Content.ReadAsStringAsync();
                MenuView.Instance.ShowPopup("无法登录: ");
                // +$"{response.StatusCode}: {errorMessage}"
                Debug.Log($"无法登陆{response.StatusCode}: {errorMessage}");

                return false;

            }
        }

        catch (HttpRequestException e)
        {
            Debug.Log($"Request error: {e.Message}");
            // 处理请求异常逻辑
        }
        catch (Exception e)
        {
            Debug.Log($"Request error: {e.Message}");
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



    public class LoginResponse
    {
        public string State { get; set; }
        public string Uid { get; set; }
    }
}