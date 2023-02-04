using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyCardZone : MonoBehaviour
{
    public AnimationCurve priceScale;
    public int total_bought;

    private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();    
    }
    public void Update()
    {
        renderer.enabled = GameManager.cash >= (int)priceScale.Evaluate(total_bought);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCar"))
        {
            if(GameManager.cash >= (int)priceScale.Evaluate(total_bought))
            {
                GameManager.Instance.Buy(priceScale.Evaluate(total_bought++));
            }
        }
    }
}
