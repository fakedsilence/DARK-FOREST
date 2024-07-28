using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathView : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("GameView");
    }

    public void Back()
    {
        SceneManager.LoadScene("MenuView");
    }
}
