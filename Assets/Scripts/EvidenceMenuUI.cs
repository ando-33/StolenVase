using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvidenceMenuUI : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button btnEvidence1;
    public Button btnEvidence2;
    public Button btnEvidence3;

    [Header("Detail Panel")]
    public GameObject detailPanel;
    public Image detailImage;
    public TMP_Text detailTitle;
    public TMP_Text detailDescription;
    public Button closeButton;

    [Header("Evidence Sprites")]
    public Sprite spAlarmLog;
    public Sprite spInnerLock;
    public Sprite spTimerDevice;

    void Start()
    {
        // 最初は閉じる
        detailPanel.SetActive(false);

        btnEvidence1.onClick.AddListener(() => ShowDetail(1));
        btnEvidence2.onClick.AddListener(() => ShowDetail(2));
        btnEvidence3.onClick.AddListener(() => ShowDetail(3));

        closeButton.onClick.AddListener(() => detailPanel.SetActive(false));
    }

    void ShowDetail(int id)
    {
        detailPanel.SetActive(true);
        switch (id)
        {
            case 1:
                detailImage.sprite = spAlarmLog;
                detailTitle.text = "警報ログ";
                detailDescription.text = "防犯システム記録。事件当時、警報は作動していなかった。";
                break;
            case 2:
                detailImage.sprite = spInnerLock;
                detailTitle.text = "内側ロック痕";
                detailDescription.text = "窓の内側からロック解除された痕跡。外部犯行を否定する証拠。";
                break;
            case 3:
                detailImage.sprite = spTimerDevice;
                detailTitle.text = "タイマー式警報装置";
                detailDescription.text = "展示台下に仕掛けられた装置。偽装工作の決定的証拠。";
                break;
        }
    }
}
