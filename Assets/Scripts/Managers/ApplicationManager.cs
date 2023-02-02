using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationManager
{
    #region Singleton
    private static ApplicationManager _instance;

    public static ApplicationManager instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = new ApplicationManager();
            }

            return _instance;
        } 
    }

    public static void DestroyInstance()
    {
        _instance.OnDestroy();
        _instance = null;
    }

    private ApplicationManager()
    {
        Awake();
    }
    #endregion

    public enum Resolutions
    {
        TWO_K = 0,
        TEN_EIGHTY,
        SEVEN_TWENTY
    };

    public int m_width;
    public int m_height;
    public int m_rate;

    public FullScreenMode m_fullscreen { get; private set; }

    public void Wake()
    {
        Debug.Log("Application manager is awake!");
    }

    /// <summary>
    /// Called imediately after creation in the constructor
    /// </summary>
    private void Awake()
    {
        m_width = PlayerPrefs.GetInt("ResWidth", Screen.currentResolution.width);
        m_height = PlayerPrefs.GetInt("ResHeight", Screen.currentResolution.height);
        m_rate = PlayerPrefs.GetInt("ResRate", Screen.currentResolution.refreshRate);
        m_fullscreen = (FullScreenMode)PlayerPrefs.GetInt("FullScreen", (int)Screen.fullScreenMode);
        UpdateResolution();
    }
    public void SaveData()
    {
        
    }

    public void SetResolution(Resolution res, bool update = false)
    {
        PlayerPrefs.SetInt("ResWidth", res.width);
        PlayerPrefs.SetInt("ResHeight", res.height);
        PlayerPrefs.SetInt("ResRate", res.refreshRate);

        m_width = res.width;
        m_height = res.height;
        m_rate = res.refreshRate;
        if (update)
        {
            UpdateResolution();
        }
    }

    public void SetFullScreen(int select, bool update = false)
    {
        PlayerPrefs.SetInt("FullScreen", select);
        m_fullscreen = (FullScreenMode)select;
        if (update)
        {
            UpdateResolution();
        }
    }

    private void UpdateResolution()
    {
        if (Screen.currentResolution.width != m_width 
            || Screen.currentResolution.height != m_height 
            || Screen.currentResolution.refreshRate != m_rate 
            || Screen.fullScreenMode != m_fullscreen)
        {
            Screen.SetResolution(m_width, m_height, m_fullscreen, m_rate);
        } 
    }

    private void OnDestroy()
    {
        SaveData();  
    }
}
