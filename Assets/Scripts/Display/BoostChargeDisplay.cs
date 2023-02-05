using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostChargeDisplay : MonoBehaviour
{
    [SerializeField] private Image charge1;
    [SerializeField] private Image charge2;
    [SerializeField] private Image charge3;

    public void UpdateValue(float _value)
    {
        charge1.fillAmount = 1.0f - (_value);
        charge2.fillAmount = 1.0f - (_value - 1.0f); 
        charge3.fillAmount = 1.0f - (_value - 2.0f); 
    }
}
