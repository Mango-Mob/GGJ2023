using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseDisplay : MonoBehaviour
{
    public void Update()
    {
        if (InputManager.Instance.IsGamepadButtonDown(ButtonType.UP, 0))
        {
            gameObject.SetActive(false);
        }
        else if (InputManager.Instance.IsGamepadButtonDown(ButtonType.DOWN, 0))
        {
            Quit();
        }
    }
    public void OnEnable()
    {
        GameManager.Instance.time_scale = 0.0f;
    }

    public void OnDisable()
    {
        if (GameManager.HasInstance())
            GameManager.Instance.time_scale = 1.0f;
    }

    public void Quit()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
