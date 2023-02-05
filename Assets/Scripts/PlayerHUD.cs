using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : Singleton<PlayerHUD>
{
    public ScoreDisplay score;
    public ScoreDisplay cash;
    public ScoreDisplay timer;

    public RewardDisplay reward;
    public PauseDisplay pause;

    public BoostChargeDisplay charges;
    public SpeedDisplay speed;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.HasInstance())
        {
            score.value = GameManager.score;
            cash.value = GameManager.cash;
            timer.value = GameManager.Instance.timer;
        }

        if ((InputManager.Instance.IsGamepadButtonDown(ButtonType.START, 0) || InputManager.Instance.IsKeyDown(KeyType.ESC)) && !reward.isActiveAndEnabled)
        {
            pause.gameObject.SetActive(!pause.isActiveAndEnabled);
        }
    }
}
