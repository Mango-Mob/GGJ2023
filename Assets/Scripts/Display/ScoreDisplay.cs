using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScoreDisplay : MonoBehaviour
{
    private TMP_Text render;

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
        render.SetText(((int)value).ToString());
    }
}
