using UnityEngine;

public class TitleBGMPlayer : MonoBehaviour
{
    void Start()
    {
        // タイトルシーン開始時にタイトルBGMを再生
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayBgm(BGMType.Title);
        }
    }
}
