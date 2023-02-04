using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    public ScoreDisplay score;
    public ScoreDisplay cash;
    public ScoreDisplay timer;

    public RewardDisplay reward;
    public PauseDisplay pause;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.HasInstance())
        {
            score.value = GameManager.Instance.score;
            cash.value = GameManager.Instance.cash;
            timer.value = GameManager.Instance.timer;
        }

        if ((InputManager.Instance.IsGamepadButtonPressed(ButtonType.START, 0) || InputManager.Instance.IsKeyDown(KeyType.ESC)) && !reward.isActiveAndEnabled)
        {
            pause.gameObject.SetActive(!pause.isActiveAndEnabled);
        }
    }
}
