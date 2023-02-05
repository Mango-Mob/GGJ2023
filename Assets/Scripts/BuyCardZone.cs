using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyCardZone : MonoBehaviour
{
    public AnimationCurve priceScale;
    public int total_bought;

    private Renderer renderer;
    private Canvas canvas;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        canvas = GetComponentInChildren<Canvas>();    
    }
    public void Update()
    {
        renderer.enabled = GameManager.cash >= (int)priceScale.Evaluate(total_bought);
        canvas.enabled = Vector3.Distance(transform.position, GameManager.Instance.m_player.transform.position) < 20 && GameManager.Instance.time_scale != 0;
        if (canvas.enabled)
        {
            if(GameManager.cash >= (int)priceScale.Evaluate(total_bought))
            {
                canvas.GetComponentInChildren<Image>().SetEnabled(true);
                canvas.GetComponentInChildren<TMP_Text>().SetEnabled(false);
            }
            else
            {
                canvas.GetComponentInChildren<Image>().SetEnabled(false);
                canvas.GetComponentInChildren<TMP_Text>().SetEnabled(true);
                canvas.GetComponentInChildren<TMP_Text>().SetText($"${(int)priceScale.Evaluate(total_bought)}");
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCar"))
        {
            other.GetComponent<Car>().HaltWheels();
            if (GameManager.cash >= (int)priceScale.Evaluate(total_bought))
            {
                GameManager.Instance.Buy(priceScale.Evaluate(total_bought++));
            }
        }
    }
}
