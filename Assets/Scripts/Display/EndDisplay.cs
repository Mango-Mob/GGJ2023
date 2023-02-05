using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDisplay : MonoBehaviour
{
    public TMP_Text score;
    public TMP_Text time;
    public TMP_Text cash;
    public TMP_Text tree;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        score.SetText($"Score: {GameManager.score}");
        tree.SetText($"Trees Collected: {GameManager.score/200}");
        time.SetText($"Elapsed Time: {ScoreDisplay.ToTimeString(GameManager.elapsed_time)}");
        cash.SetText($"Remaining Cash: ${GameManager.cash}");
    }
    public void Play()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void Quit()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void Update()
    {
        if (InputManager.Instance.IsGamepadButtonDown(ButtonType.UP, 0))
        {
            Play();
        }
        else if (InputManager.Instance.IsGamepadButtonDown(ButtonType.DOWN, 0))
        {
            Quit();
        }
    }
}
