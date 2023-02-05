using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDisplay : MonoBehaviour
{
    public GameObject keyboard;
    public GameObject controller;

    // Update is called once per frame
    void Update()
    {
        keyboard.SetActive(!InputManager.Instance.isInGamepadMode);
        controller.SetActive(InputManager.Instance.isInGamepadMode);
    }
}
