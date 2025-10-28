using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    [Header("BGM Clips")]
    public AudioClip titleBGM;
    public AudioClip stageBGM;
    public AudioClip endingBGM;

    [Header("Audio Source")]
    public AudioSource audioSource;

    private string currentScene = "";
    private bool isManualPlay = false; // ← 手動でBGMを切り替えたフラグ

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void Start()
    {
        PlayBGMForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // まず必ず古いシーンの音を止める
        if (audioSource.isPlaying)
            audioSource.Stop();

        // EndingBGM再生中は上書きしない
        if (isManualPlay) return;

        PlayBGMForScene(newScene.name);
    }


    private void PlayBGMForScene(string sceneName)
    {
        if (audioSource == null) return;

        // もしAudioSourceが無効なら自動で有効化
        if (!audioSource.enabled) audioSource.enabled = true;

        currentScene = sceneName;

        AudioClip nextClip = null;

        if (sceneName == "Title")
            nextClip = titleBGM;
        else if (sceneName == "Main")
            nextClip = stageBGM;

        if (nextClip == null)
            return;

        if (audioSource.clip != nextClip)
        {
            audioSource.Stop();
            audioSource.clip = nextClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // ✅ 手動でBGMを再生する（シーン切替に関係なく流れる）
    public void PlayEndingBGM()
    {
        if (audioSource == null || endingBGM == null) return;

        isManualPlay = true; // ← 手動再生モードON
        audioSource.Stop();
        audioSource.clip = endingBGM;
        audioSource.loop = true;
        audioSource.Play();
    }

    // ✅ 元の自動切替に戻す
    public void ResumeSceneBGM()
    {
        isManualPlay = false;
        PlayBGMForScene(currentScene);
    }
}
