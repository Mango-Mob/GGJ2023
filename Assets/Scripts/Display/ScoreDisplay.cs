using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScoreDisplay : MonoBehaviour
{
    private TMP_Text render;
    public bool is_Time = false;
    public bool is_cash = false;
    [Range(0f, 9999f)]
    public float value;
    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<TMP_Text>();
        render.SetText(((int)value).ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_Time && !is_cash)
            render.SetText(((int)value).ToString());
        else if(is_cash)
            render.SetText($"${value}");
        else
        {
            render.SetText(ToTimeString(value));
            if (value <= 30)
                render.color = Color.red;
            else
                render.color = Color.white;
        }
    }

    public static string ToTimeString(float value)
    {
        if (value <= 0)
            return $"00:00";

        int mins = 0;
        if(value > 60)
        {
            mins = (int)value / 60; //61
            value = value % 60;
        }

        return $"{mins.ToString("D2")}:{((int)value).ToString("D2")}";
    }
}
