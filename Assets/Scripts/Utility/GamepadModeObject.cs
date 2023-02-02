using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamepadModeObject : MonoBehaviour
{
    public GameObject m_targetObject;
    public bool m_hideInGamepadMode = false;
    void Update()
    {
        if (m_hideInGamepadMode)
            m_targetObject.SetActive(!InputManager.Instance.isInGamepadMode);
        else
            m_targetObject.SetActive(InputManager.Instance.isInGamepadMode);

    }
}
