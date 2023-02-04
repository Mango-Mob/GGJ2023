﻿using System;
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
        if(InputManager.Instance.IsKeyPressed(KeyType.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    internal void AddCash(int _cash)
    {
        cash += _cash;
    }
}
