using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXTimerScript : MonoBehaviour
{
    public bool m_startedTimer = true;
    [Tooltip("In seconds.")]
    public float m_timer = 1.0f;

    // Update is called once per frame
    protected virtual void Update()
    {
        if (m_startedTimer)
        {
            m_timer -= Time.deltaTime;

            if (m_timer <= 0.0f)
                Destroy(gameObject);
        }
    }

    public virtual void Finish()
    {
        m_timer = 0;
    }
}
