using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlayScene")
        {
            GameController.Instance.InitializePlayScene();
        }
    }

    public void LoadLevel()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}