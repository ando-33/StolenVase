using UnityEngine;

public class MainBGMPlayer : MonoBehaviour
{
    void Start()
    {
        // メインシーン開始時にBGMを再生
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayBgm(BGMType.InGame);
        }
    }
}
