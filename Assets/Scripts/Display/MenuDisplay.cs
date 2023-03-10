using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuDisplay : MonoBehaviour
{
    public GameObject volume;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.IsGamepadButtonDown(ButtonType.UP, 0))
        {
            Play();
        }
        else if (InputManager.Instance.IsGamepadButtonDown(ButtonType.LEFT, 0))
        {
            Settings();
        }
        else if (InputManager.Instance.IsGamepadButtonDown(ButtonType.DOWN, 0))
        {
            Quit();
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void Settings()
    {
        Animator anim = volume.GetComponentInParent<Animator>();
        if (volume.activeInHierarchy && !anim.GetCurrentAnimatorStateInfo(0).IsName("Close"))
        {
            anim.SetTrigger("Close");
        }
        else if (!volume.activeInHierarchy && !anim.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            anim.SetTrigger("Open");
        }
    }
    public void Logo()
    {
        Application.OpenURL("https://mangomob.itch.io/");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
