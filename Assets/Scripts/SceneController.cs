using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadLevel()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlayScene")
        {
            GameController.Instance.InitializePlayScene();
        }
    }
}