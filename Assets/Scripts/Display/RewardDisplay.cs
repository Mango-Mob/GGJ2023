using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardDisplay : MonoBehaviour
{
    public List<Modifier> allOptions;
    private List<Modifier> casheOptions = new List<Modifier>();
    public OptionDisplay option1;
    public OptionDisplay option2;
    public OptionDisplay option3;

    public void Start()
    {
        casheOptions.AddRange(allOptions);
        allOptions.Sort((a, b) => 1 - 2 * Random.Range(0, 2));
    }
    public void Update()
    {
        if (InputManager.Instance.IsGamepadButtonPressed(ButtonType.LEFT, 0))
        {
            option1.GiveReward();
            gameObject.SetActive(false);
        }
        else if (InputManager.Instance.IsGamepadButtonPressed(ButtonType.UP, 0))
        {
            option2.GiveReward();
            gameObject.SetActive(false);
        }
        else if (InputManager.Instance.IsGamepadButtonPressed(ButtonType.RIGHT, 0))
        {
            option3.GiveReward();
            gameObject.SetActive(false);
        }
    }
    public void OnEnable()
    {
        if(casheOptions.Count == 0)
        {
            casheOptions.AddRange(allOptions);
            allOptions.Sort((a, b) => 1 - 2 * Random.Range(0, 2));
        }
        GameManager.Instance.time_scale = 0.0f;
        int select = Random.Range(0, allOptions.Count);
        option1.modifier = allOptions[select];
        allOptions.RemoveAt(select);

        select = Random.Range(0, allOptions.Count);
        option2.modifier = allOptions[select];
        allOptions.RemoveAt(select);

        select = Random.Range(0, allOptions.Count);
        option3.modifier = allOptions[select];
        allOptions.RemoveAt(select);

        if(allOptions.Count < 3)
        {
            allOptions.Clear();
            allOptions.AddRange(casheOptions);
            allOptions.Sort((a, b) => 1 - 2 * Random.Range(0, 2));
        }
    }

    public void OnDisable()
    {
        if(GameManager.HasInstance())
            GameManager.Instance.time_scale = 1.0f;
    }
}
