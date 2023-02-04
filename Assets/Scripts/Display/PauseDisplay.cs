using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseDisplay : MonoBehaviour
{
    public void OnEnable()
    {
        GameManager.Instance.time_scale = 0.0f;
    }

    public void OnDisable()
    {
        if (GameManager.HasInstance())
            GameManager.Instance.time_scale = 0.0f;
    }

    public void Quit()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
