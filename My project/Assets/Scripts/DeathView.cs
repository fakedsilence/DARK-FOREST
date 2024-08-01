using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathView : MonoBehaviour
{
    public GameObject deathScore;
    private TextMeshProUGUI deathScoreText;

    private void Awake()
    {
        // ȷ�� TextMeshProUGUI ����Ѿ�����ȷ����
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

    // ������Ϸ
    public void Restart()
    {
        SceneManager.LoadScene("GameView");
    }

    // �������˵�
    public void Back()
    {
        Application.Quit();
    }
}