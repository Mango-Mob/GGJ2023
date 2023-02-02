using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Timer
{
    public float m_timeRemaining;
    public bool m_unscaled = false;
    public UnityEvent onFinish = new UnityEvent();

    private bool m_started = false;

    public void Start(float initial)
    {
        m_started = initial > 0;
        m_timeRemaining = initial;
    }

    public void Stop()
    {
        m_started = false;
        m_timeRemaining = 0f;
    }

    public void Update()
    {
        if(m_started)
            m_timeRemaining -= (m_unscaled) ? Time.unscaledDeltaTime: Time.deltaTime;

        if (m_started && m_timeRemaining <= 0)
        {
            m_started = false;
            onFinish.Invoke();
        }
    }
}
