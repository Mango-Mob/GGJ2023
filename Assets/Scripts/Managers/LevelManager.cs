using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// William de Beer
/// </summary>
public class LevelManager : SingletonPersistent<LevelManager>
{
    public enum Transition
    {
        CROSSFADE,
        CROSSFADE_SPLIT,
        YOUDIED,
        YOUWIN
    }

    public static bool cheatsEnabled = false;
    public static bool loadingNextArea = false;

    public GameObject loadingBarPrefab;
    public GameObject loadingHintPrefab;

    public static GameObject transitionPrefab;
    public static GameObject youdiedPrefab;
    public static GameObject youwinPrefab;
    public static Animator transition;

    public bool isTransitioning = false;
    public float transitionTime = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        //SceneManager.sceneLoaded += OnSceneLoaded;
        transitionPrefab = Resources.Load<GameObject>("Transitions/TransitionCanvas");
        youdiedPrefab = Resources.Load<GameObject>("Transitions/YouDiedCanvas");
        youwinPrefab = Resources.Load<GameObject>("Transitions/YouWinCanvas");
    }

    private void Start()
    {
        loadingNextArea = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().name));
    }
    public void LoadNextLevel()
    {
        loadingNextArea = true;
        if (SceneManager.sceneCountInBuildSettings <= SceneManager.GetActiveScene().buildIndex + 1) // Check if index exceeds scene count
        {
            StartCoroutine(LoadLevel(SceneManager.GetSceneByBuildIndex(0).name)); // Load menu
        }
        else
        {
            StartCoroutine(LoadLevel(SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1).name)); // Loade next scene
        }
    }
    public void LoadNewLevel(string _name, Transition _transition = Transition.CROSSFADE)
    {
        if (!isTransitioning)
            StartCoroutine(LoadLevel(_name, _transition));
    }
    public void ResetScene()
    {
        loadingNextArea = true;
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().name));
    }

    IEnumerator LoadLevel(string _name, Transition _transition = Transition.CROSSFADE)
    {
        float timeMult = 1.0f;
        isTransitioning = true;

        switch (_transition)
        {
            case Transition.CROSSFADE:
                transition = Instantiate(transitionPrefab, transform).GetComponent<Animator>();
                break;
            case Transition.CROSSFADE_SPLIT:
                transition = Instantiate(transitionPrefab, transform).GetComponent<Animator>();
                Time.timeScale = 1.0f;
                timeMult = 0.1f;
                break;
            case Transition.YOUDIED:
                transition = Instantiate(youdiedPrefab, transform).GetComponent<Animator>();
                timeMult = 5.0f;
                break;
            case Transition.YOUWIN:
                transition = Instantiate(youwinPrefab, transform).GetComponent<Animator>();
                timeMult = 3.5f;
                break;
        }

        transition.speed = 1.0f / timeMult;

        if (transition != null)
        {
            // Wait to let animation finish playing
            yield return new WaitForSeconds(transitionTime * timeMult);
        }

        transition.speed = 0.0f;

        // Get random hint
        TextMeshProUGUI loadingHint = Instantiate(loadingHintPrefab, transition.transform).GetComponentInChildren<TextMeshProUGUI>();
        loadingHint.transform.SetAsLastSibling();

        TextAsset hintList = Resources.Load<TextAsset>("UI/LoadingHints");
        string[] lines = hintList.text.Split('\n');
        int randomIndex = UnityEngine.Random.Range(0, lines.Length);

        loadingHint.text = "Hint: " + lines[randomIndex];

        Animator hintAnimator = loadingHint.GetComponentInChildren<Animator>();

        // Loading screen
        Slider loadingBar = Instantiate(loadingBarPrefab, transition.transform).GetComponent<Slider>();
        loadingBar.transform.SetAsLastSibling();
        AsyncOperation gameLoad = SceneManager.LoadSceneAsync(_name);

        while (!gameLoad.isDone)
        {
            float progress = Mathf.Clamp01(gameLoad.progress / 0.9f);
            Debug.Log(gameLoad.progress);

            if (loadingBar)
            {
                loadingBar.value = progress;
            }

            yield return new WaitForEndOfFrame();
        }

        if (_transition == Transition.CROSSFADE_SPLIT || _transition == Transition.YOUDIED)
            timeMult = 1.0f;

        transition.speed = 1.0f / timeMult;

        hintAnimator.speed = 1.0f / timeMult;
        hintAnimator.SetTrigger("Fade");

        Destroy(loadingBar.gameObject);

        //SceneManager.LoadScene(_name);
        yield return new WaitForSeconds(transitionTime * timeMult);

        if (transition != null)
        {
            Destroy(transition.gameObject);
            transition = null;
        }
        isTransitioning = false;
        yield return null;
    }
    //IEnumerator LoadLevelAsync(string _name)
    //{
    //    AsyncOperation gameLoad = SceneManager.LoadSceneAsync(_name);
    //    gameLoad.allowSceneActivation = false;
    //    float time = 0.0f;

    //    while (!gameLoad.isDone && isTransitioning == false)
    //    {
    //        gameLoad.progress

    //        time += Time.deltaTime;
    //        if (gameLoad.progress >= 0.9f)
    //        {
    //            CompleteLoadUI.SetActive(true);

    //        }
    //        yield return new WaitForEndOfFrame();
    //    }

    //    CompleteLoadUI.SetActive(false);
    //    yield return null;
    //}

    //public void LoadingScreenLoad(int levelIndex, float maxTime)
    //{
    //    StartCoroutine(LoadLevelAsync(levelIndex, maxTime));
    //}

    //IEnumerator OperationLoadLevelAsync(int levelIndex, float maxTime)
    //{
    //    AsyncOperation gameLoad = SceneManager.LoadSceneAsync(levelIndex);
    //    gameLoad.allowSceneActivation = false;
    //    float time = 0.0f;

    //    while (!gameLoad.isDone)
    //    {
    //        time += Time.deltaTime;
    //        if (gameLoad.progress >= 0.9f)
    //        {
    //            CompleteLoadUI.SetActive(true);

    //            if (InputManager.Instance.IsGamepadButtonDown(ButtonType.SOUTH, 0))
    //            {
    //                gameLoad.allowSceneActivation = true;
    //            }
    //            if (time >= maxTime)
    //            {
    //                gameLoad.allowSceneActivation = true;
    //            }
    //        }
    //        yield return new WaitForEndOfFrame();
    //    }

    //    CompleteLoadUI.SetActive(false);
    //    yield return null;
    //}

    //public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    if (GameManager.HasInstance())
    //    {
    //        var objects = GameManager.Instance.m_saveSlot.GetSceneData(scene.buildIndex);

    //        if (objects == null || objects.Length == 0)
    //            return;

    //        foreach (var item in objects)
    //        {
    //            if (item != null)
    //            {
    //                int id = item.m_itemID;
    //                GameObject prefab = Resources.Load<GameObject>(GameManager.Instance.m_items.list[id].placePrefabName);

    //                GameObject inWorld = Instantiate(prefab, new Vector3(item.x, item.y, item.z), Quaternion.Euler(item.rx, item.ry, item.rz));
    //                inWorld.GetComponent<SerializedObject>().UpdateTo(item);
    //            }
    //        }

    //        GameManager.Instance.m_saveSlot.InstansiateNPCs(scene.buildIndex);
    //    }
    //}

    //public void SaveSceneToSlot(SaveSlot slot)
    //{
    //    slot.SaveObjects(GameObject.FindGameObjectsWithTag("SerializedObject"));
    //    foreach (var item in GameObject.FindGameObjectsWithTag("NPC"))
    //    {
    //        slot.AddNPC(item.GetComponent<NPCScript>());
    //    }
    //}
}
