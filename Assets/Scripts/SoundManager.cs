using UnityEngine;
using UnityEngine.SceneManagement;

public enum BGMType
{
    None,
    Title,
    InGame,
    End,
}

public enum SEType
{
    Present,
    Click,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    BGMType playingBGM;

    // 🎵 BGM用とSE用を分ける
    private AudioSource bgmSource;
    private AudioSource seSource;

    public AudioClip titleBGM;
    public AudioClip stageBGM;
    public AudioClip endingBGM;

    public AudioClip sePresent;
    public AudioClip seClick;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSourceを2つ用意
        var sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            // もし1つしかなければ追加で作る
            bgmSource = gameObject.AddComponent<AudioSource>();
            seSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            bgmSource = sources[0];
            seSource = sources[1];
        }

        bgmSource.loop = true;
    }

    public void PlayBgm(BGMType type)
    {
        if (type != playingBGM)
        {
            playingBGM = type;

            switch (type)
            {
                case BGMType.Title:
                    bgmSource.clip = titleBGM;
                    bgmSource.Play();
                    break;
                case BGMType.InGame:
                    bgmSource.clip = stageBGM;
                    bgmSource.Play();
                    break;
                case BGMType.End:
                    bgmSource.clip = endingBGM;
                    bgmSource.Play();
                    break;
            }
        }
    }

    public void PlaySE(SEType type)
    {
        switch (type)
        {
            case SEType.Present:
                if (sePresent) seSource.PlayOneShot(sePresent);
                break;
            case SEType.Click:
                if (seClick) seSource.PlayOneShot(seClick);
                break;
        }
    }

    public void StopBgm()
    {
        bgmSource.Stop();
        playingBGM = BGMType.None;
    }
}
