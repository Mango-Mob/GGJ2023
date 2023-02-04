using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public GameObject m_player;
    public Camera m_activeCamera;
    public bool IsInCombat = false;

    public float cash;
    public float score;
    public float timer = 3 * 60f;

    public float time_scale = 1.0f;
    protected override void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_activeCamera = Camera.main;
    }

    private void OnLevelWasLoaded(int level)
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_activeCamera = Camera.main;
    }
    public void Update()
    {
        timer -= Time.deltaTime * time_scale;

        Cursor.lockState = (Cursor.visible) ? CursorLockMode.None: CursorLockMode.Locked;
        Cursor.visible = time_scale == 0f;
        if(timer <= 0)
        {
            //game Over
        }
    }

    internal void AddCash(int _cash)
    {
        cash += _cash;
        timer += (_cash / 100) * 15f;
        score += _cash;
    }
}
