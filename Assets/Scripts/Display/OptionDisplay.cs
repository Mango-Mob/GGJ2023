using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OptionDisplay : MonoBehaviour
{
    public Modifier modifier;

    public TMP_Text top_text;
    public TMP_Text bottem_text;

    // Update is called once per frame
    void Update()
    {
        if(modifier)
        {
            top_text.SetText(Modifier.DisplayStat(modifier.Changes[0]));
            if (modifier.Changes.Count != 1)
                bottem_text.SetText(Modifier.DisplayStat(modifier.Changes[1]));
            else
                bottem_text.SetText("Nothing");
        }
    }

    public void GiveReward()
    {

    }
}
