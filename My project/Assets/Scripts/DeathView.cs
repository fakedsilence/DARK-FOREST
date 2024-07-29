using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathView : MonoBehaviour
{
    public GameObject deathScore;
    private TextMeshProUGUI deathScoreText;

    private void Awake()
    {
        // 确保 TextMeshProUGUI 组件已经被正确关联
        deathScoreText = deathScore.GetComponent<TextMeshProUGUI>();
        if (deathScoreText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on deathScore object.");
        }
    }

    private void OnEnable()
    {
        UpdateDeathScore();
    }

    private void UpdateDeathScore()
    {
        if (deathScoreText != null)
        {
            deathScoreText.text = "Score: " + GameMainView.score.ToString();
        }
    }

    // 重启游戏
    public void Restart()
    {
        SceneManager.LoadScene("GameView");
    }

    // 返回主菜单
    public void Back()
    {
        SceneManager.LoadScene("MenuView");
    }
}