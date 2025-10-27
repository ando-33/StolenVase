using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleButton : MonoBehaviour
{
    // ボタンをクリックした時に呼ばれる関数
    public void OnClickStartButton()
    {
        // メインシーンへ切り替え
        SceneManager.LoadScene("MainScene");
    }
}
