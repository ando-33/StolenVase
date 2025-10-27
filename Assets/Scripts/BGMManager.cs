using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    [Header("BGM Clips")]
    public AudioClip titleBGM;
    public AudioClip stageBGM;

    [Header("Audio Source")]
    public AudioSource audioSource;

    private void Awake()
    {
        // すでに存在しているBGMManagerがあれば破棄する（古い方を残す）
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 最初の1つだけ残す
        instance = this;
        DontDestroyOnLoad(gameObject);

        // シーン切り替え監視を1回だけ登録
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void Start()
    {
        PlayBGMForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        PlayBGMForScene(newScene.name);
    }

    private void PlayBGMForScene(string sceneName)
    {
        if (audioSource == null) return;

        AudioClip nextClip = null;

        if (sceneName == "Title")
            nextClip = titleBGM;
        else if (sceneName == "Main")
            nextClip = stageBGM;

        if (nextClip == null)
            return;

        // 違う曲に切り替えるときだけ再生
        if (audioSource.clip != nextClip)
        {
            audioSource.Stop();
            audioSource.clip = nextClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
