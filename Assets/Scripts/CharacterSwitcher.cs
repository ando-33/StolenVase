using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
    }

    [Header("UI References")]
    public Image lawyerImage;
    public Image guardImage;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Button nextButton;

    [Header("Choices")]
    public GameObject choicesPanel;
    public Button choice1Button; // ← 正解（のみ進行）
    public Button choice2Button; // ← 不正解 → ゲームオーバー
    public Button choice3Button; // ← 不正解 → ゲームオーバー

    [Header("Game Over UI")]
    public GameObject gameOverPanel;   // パネル（「Game Over」「やり直す」ボタン）
    public TMP_Text gameOverText;
    public Button retryButton;

    [Header("Dialogue Lines")]
    public DialogueLine startQuestion;     // 最初の弁護士セリフ
    public DialogueLine guardNextLine;     // Choice1正解時に進むガードの次セリフ

    private enum State { Question, Choice, AfterCorrect, End }
    private State state = State.Question;

    void Start()
    {
        // 初期表示
        ShowLawyerLine(startQuestion.text);

        nextButton.onClick.AddListener(ShowChoices);
        choicesPanel.SetActive(false);

        // GameOver UI 初期化
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (retryButton) retryButton.onClick.AddListener(RestartScene);
    }

    void ShowLawyerLine(string text)
    {
        nameText.text = "Lawyer";
        dialogueText.text = text;
        lawyerImage.enabled = true;
        guardImage.enabled = false;
    }

    void ShowGuardLine(string text)
    {
        nameText.text = "Guard";
        dialogueText.text = text;
        lawyerImage.enabled = false;
        guardImage.enabled = true;
    }

    // ── 選択肢表示 ─────────────────────────────
    void ShowChoices()
    {
        state = State.Choice;
        nextButton.gameObject.SetActive(false);
        choicesPanel.SetActive(true);

        // ボタン表示文（必要なら自由に変更）
        SetBtn(choice1Button, "（A）展示室の内側でロックを外した"); // ← 正解
        SetBtn(choice2Button, "（B）監視カメラの映像を見ていた");
        SetBtn(choice3Button, "（C）廊下を巡回していた");

        // 既存のリスナーを掃除
        choice1Button.onClick.RemoveAllListeners();
        choice2Button.onClick.RemoveAllListeners();
        choice3Button.onClick.RemoveAllListeners();

        // 正解のみ進行
        choice1Button.onClick.AddListener(OnCorrectChoice);

        // 不正解はゲームオーバー
        choice2Button.onClick.AddListener(() => OnWrongChoice("証言は破綻した…尋問はここで終了だ。"));
        choice3Button.onClick.AddListener(() => OnWrongChoice("核心を外した！もう弁護は続けられない…。"));
    }

    void SetBtn(Button b, string t)
    {
        var txt = b.GetComponentInChildren<TMP_Text>();
        if (txt) txt.text = t;
    }

    // ── 正解時：次のガード台詞へ ───────────────
    void OnCorrectChoice()
    {
        choicesPanel.SetActive(false);
        nextButton.gameObject.SetActive(true);

        ShowGuardLine(guardNextLine.text); // ここで動揺ライン等に差し替え可
        state = State.AfterCorrect;

        // Nextを押すと以降の流れへ（必要に応じて別メソッドで詰問→自白に接続）
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            dialogueText.text = "(この後の詰問／証拠提示へ続く…)";
            nextButton.interactable = false; // ここから先はあなたの既存ロジックに接続してOK
        });
    }

    // ── 不正解：ゲームオーバー ─────────────────
    void OnWrongChoice(string reason)
    {
        choicesPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameOverText) gameOverText.text = $"Game Over\n{reason}";

        // 以降の入力を止める
        nextButton.gameObject.SetActive(false);
    }

    // ── リトライ ───────────────────────────────
    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
