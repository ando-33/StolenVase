using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// CharacterSwitcher（クリックで進行＋証拠表示付き＋背景対応）
/// </summary>
public class CharacterSwitcher : MonoBehaviour
{
    [Header("UI")]
    public Image backgroundImage;
    public Image characterLeft;
    public Image characterRight;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("クリックヒント（On Click）")]
    public TMP_Text clickHintText;

    [Header("Choices Panel")]
    public GameObject choicesPanel;
    public TMP_Text choicesTitleText;
    public Button choice1Button;
    public Button choice2Button;
    public Button choice3Button;
    public Button choice4Button;
    public TMP_Text choicesHint;

    [Header("Evidence")]
    public Button presentButton;
    public GameObject evidencePanel;
    public Button evAlarmBtn;
    public Button evInnerLockBtn;
    public Button evDeviceBtn;
    public Image evidencePreview;  // 証拠プレビュー（パネル内）
    public TMP_Text evidenceCaption;
    public TMP_Text evidenceHint;

    [Header("Cut-in (証拠表示用)")]
    public Image cutInImage;  // 証拠画像の拡大表示（Canvas直下に設置）

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public Button retryButton;

    [Header("Sprites")]
    public Sprite curatorNormal, cleanerNormal, guardNormal, guardNervous;
    public Sprite directorNormal, directorConfess;
    public Sprite detectiveSprite;

    [Header("Evidence Sprites")]
    public Sprite spAlarmLog;
    public Sprite spInnerLock;
    public Sprite spTimerDevice;

    [Header("SFX (Optional)")]
    public AudioSource sfx;
    public AudioClip sfxReveal, sfxConfess;

    private bool heardCurator, heardCleaner, heardGuard;

    void Start()
    {
        // ===== 初期化 =====
        choicesPanel.SetActive(false);
        evidencePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        presentButton.gameObject.SetActive(false);

        if (evidenceHint) evidenceHint.text = "";
        if (choicesHint) choicesHint.text = "";
        if (clickHintText) clickHintText.gameObject.SetActive(false);
        if (choicesTitleText) choicesTitleText.gameObject.SetActive(false);

        // 🔹 証拠画像は常に非表示スタート
        if (evidencePreview) evidencePreview.gameObject.SetActive(false);
        if (cutInImage) cutInImage.gameObject.SetActive(false);

        // 背景設定
        if (backgroundImage)
            backgroundImage.enabled = true;

        // 探偵（左側）
        if (characterLeft && detectiveSprite)
        {
            characterLeft.enabled = true;
            characterLeft.sprite = detectiveSprite;
        }

        retryButton.onClick.AddListener(() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));

        evAlarmBtn.onClick.AddListener(() => OnEvidence(1));
        evInnerLockBtn.onClick.AddListener(() => OnEvidence(2));
        evDeviceBtn.onClick.AddListener(() => OnEvidence(3));
        presentButton.onClick.AddListener(() => OpenEvidencePanel());

        StartCoroutine(IntroSequence());
    }

    // --- 冒頭ナレーション ---
    IEnumerator IntroSequence()
    {
        Narration("美術館で高価な壺が“盗まれた”。まずは関係者から事情を聴こう。");
        yield return new WaitForSeconds(4.5f);
        yield return WaitForClick();
        ShowMeetSuspectsMenu();
    }

    // --- 汎用クリック待ち ---
    IEnumerator WaitForClick()
    {
        if (clickHintText) clickHintText.gameObject.SetActive(true);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        if (clickHintText) clickHintText.gameObject.SetActive(false);
        yield return null;
    }

    // ===== 3人の供述選択 =====
    void ShowMeetSuspectsMenu()
    {
        nameText.text = "";
        dialogueText.text = "";
        choicesPanel.SetActive(true);
        if (choicesHint) choicesHint.text = "";
        if (choicesTitleText) choicesTitleText.gameObject.SetActive(false);
        if (choice4Button) choice4Button.gameObject.SetActive(false);

        SetBtn(choice1Button, "学芸員の供述を聴く");
        SetBtn(choice2Button, "清掃員の供述を聴く");
        SetBtn(choice3Button, "警備員の供述を聴く");
        ClearChoiceListeners();

        choice1Button.onClick.AddListener(() =>
        {
            StartCoroutine(SuspectDialogue("学芸員", curatorNormal,
                "展示準備で手いっぱいだったわ。壺の展示室には、その時間は入っていないわ。"));
            heardCurator = true;
        });

        choice2Button.onClick.AddListener(() =>
        {
            StartCoroutine(SuspectDialogue("清掃員", cleanerNormal,
                "閉館後は廊下の掃除をしました。展示室の中は入っていませんよ。"));
            heardCleaner = true;
        });

        choice3Button.onClick.AddListener(() =>
        {
            StartCoroutine(SuspectDialogue("警備員", guardNormal,
                "“警報音を聞いて”駆け付けたときには、壺はもう無かった。窓ガラスを割って犯人は侵入したようだ。"));
            heardGuard = true;
        });
    }

    IEnumerator SuspectDialogue(string name, Sprite img, string text)
    {
        choicesPanel.SetActive(false);
        ShowRightCharacter(img, name, text);
        yield return WaitForClick();

        if (heardCurator && heardCleaner && heardGuard)
            StartCoroutine(Start_A_GuardTestimony());
        else
        {
            Narration("他の供述も確認しておこう。");
            yield return WaitForClick();
            ShowMeetSuspectsMenu();
        }
    }

    // ===== 警備員証言 =====
    IEnumerator Start_A_GuardTestimony()
    {
        ShowLeftCharacter("探偵", "本当に警報音を聞いたのですか？", keepRight: true);
        yield return WaitForClick();

        ShowRightCharacter(guardNormal, "警備員", "確かに“警報音を聞いた”から展示室へ急行したんだ。");
        yield return WaitForClick();

        Show_AlarmEvidence();
    }

    void Show_AlarmEvidence()
    {
        ShowLeftCharacter("探偵", "防犯システムの記録では、該当時刻に“警報作動なし”。あなたが聞いたのは何の音ですか？");
        StartCoroutine(AlarmEvidenceResponse());
    }

    IEnumerator AlarmEvidenceResponse()
    {
        PlaySE(sfxReveal);
        yield return WaitForClick();
        ShowRightCharacter(guardNervous, "警備員", "な、なに？そんなはずは……！でも確かに音がしたんだ。展示室に私が着いた時には警報は止まっていたが。");
        yield return WaitForClick();
        StartCoroutine(Start_B_OuterClaim());
    }

    IEnumerator Start_B_OuterClaim()
    {
        Narration("“外から割られた”という主張。しかし現場には——");
        yield return WaitForClick();
        presentButton.gameObject.SetActive(true);
    }

    // ===== 証拠提示 =====
    void OpenEvidencePanel()
    {
        if (evidenceHint) evidenceHint.text = "";
        if (nameText) nameText.gameObject.SetActive(false);
        if (dialogueText) dialogueText.gameObject.SetActive(false);

        evidencePanel.SetActive(true);
        if (evidencePreview) evidencePreview.gameObject.SetActive(true);

        UpdateEvidencePreview(0);
    }

    void OnEvidence(int id)
    {
        UpdateEvidencePreview(id);

        // 🔹 証拠2「内側ロック痕」を選んだら専用演出
        if (id == 2)
        {
            StartCoroutine(ShowInnerLockEvidenceCutIn());
        }
        else
        {
            ShowEvidenceWrong("提示する証拠が違うようです。もう一度選んでください。");
        }
    }

    // 🔹 証拠2：内側ロック痕を表示するカットイン演出
    IEnumerator ShowInnerLockEvidenceCutIn()
    {
        // まずパネルを閉じてUIを隠す
        evidencePanel.SetActive(false);
        if (nameText) nameText.gameObject.SetActive(false);
        if (dialogueText) dialogueText.gameObject.SetActive(false);
        if (clickHintText) clickHintText.gameObject.SetActive(false);

        // 🔸 証拠画像表示
        if (cutInImage)
        {
            cutInImage.gameObject.SetActive(true);
            cutInImage.sprite = spInnerLock;
            cutInImage.preserveAspect = true;
        }

        PlaySE(sfxReveal);
        yield return WaitForClick(); // ← 左クリックで進む

        // 🔸 証拠画像を非表示
        if (cutInImage)
        {
            cutInImage.sprite = null;
            cutInImage.gameObject.SetActive(false);
        }

        // 通常のセリフに戻す
        if (nameText) nameText.gameObject.SetActive(true);
        if (dialogueText) dialogueText.gameObject.SetActive(true);

        // 証拠OK後の進行
        EvidenceOK("探偵", "窓は“内側”からロック解除の痕跡。外部犯行に見せかけた偽装だ。");
        yield return WaitForClick();
        StartCoroutine(Start_PreChoiceBait());
    }

    void EvidenceOK(string speaker, string text)
    {
        if (evidencePreview) evidencePreview.gameObject.SetActive(false);
        presentButton.gameObject.SetActive(false);
        ShowLeftCharacter(speaker, text);
    }

    void ShowEvidenceWrong(string hint)
    {
        if (evidenceHint) evidenceHint.text = hint;
    }

    // ===== 館長パート =====
    IEnumerator Start_PreChoiceBait()
    {
        ShowLeftCharacter("探偵", "館長、展示室で壺が盗まれた時の状況を教えてください。");
        yield return WaitForClick();

        ShowRightCharacter(directorNormal, "館長", "警備員に呼ばれて展示室に向かうと、展示台で鳴った警報は止まっておった。");
        yield return WaitForClick();

        ShowCulpritChoices4();
    }

    // ===== 犯人選択（4択） =====
    void ShowCulpritChoices4()
    {
        nameText.text = "";
        dialogueText.text = "";
        choicesPanel.SetActive(true);
        if (choicesHint) choicesHint.text = "";

        if (choicesTitleText)
        {
            choicesTitleText.text = "事件の犯人を選んでください";
            choicesTitleText.gameObject.SetActive(true);
        }

        if (choice4Button) choice4Button.gameObject.SetActive(true);

        SetBtn(choice1Button, "学芸員");
        SetBtn(choice2Button, "清掃員");
        SetBtn(choice3Button, "警備員");
        SetBtn(choice4Button, "この中にいない");

        ClearChoiceListeners();

        choice1Button.onClick.AddListener(() => choicesHint.text = "学芸員は疑う点がない。もう一度選んでください。");
        choice2Button.onClick.AddListener(() => choicesHint.text = "清掃員は展示室に入っていない。もう一度選んでください。");
        choice3Button.onClick.AddListener(() => choicesHint.text = "決定的な証拠がない。もう一度選んでください。");
        choice4Button.onClick.AddListener(() =>
        {
            choicesPanel.SetActive(false);
            if (choicesTitleText) choicesTitleText.gameObject.SetActive(false);
            StartCoroutine(Show_Confession());
        });
    }

    // ===== 自白 =====
    IEnumerator Show_Confession()
    {
        ShowLeftCharacter("探偵", "真犯人は館長、あなたです。今、あなたは警報音が展示“台”から鳴っていたことを知っていた。展示台の下に仕掛けた“タイマー式警報装置”が証拠です。");
        PlaySE(sfxReveal);
        yield return WaitForClick();

        yield return ShowDeviceEvidenceCutIn();

        ShowRightCharacter(directorConfess, "館長", "……認めよう。私は壺を割ってしまった。それで外部犯行に見せかけるために装置を仕掛けたのじゃ。");
        PlaySE(sfxConfess);
        yield return WaitForClick();

        Narration("事件は解決。壺は館長自身の手で壊され、贋作を隠すための偽装事件だった。");
        yield return WaitForClick();

        dialogueText.text = "（お疲れさまでした）";
    }

    // ===== 「これだ！」演出（証拠3：タイマー装置） =====
    IEnumerator ShowDeviceEvidenceCutIn()
    {
        if (characterLeft) characterLeft.enabled = false;
        if (characterRight) characterRight.enabled = false;
        if (nameText) nameText.gameObject.SetActive(false);
        if (dialogueText) dialogueText.gameObject.SetActive(false);
        if (clickHintText) clickHintText.gameObject.SetActive(false);

        if (cutInImage)
        {
            cutInImage.gameObject.SetActive(true);
            cutInImage.sprite = spTimerDevice;
            cutInImage.preserveAspect = true;
        }

        PlaySE(sfxReveal);
        yield return WaitForClick();

        if (cutInImage)
        {
            cutInImage.sprite = null;
            cutInImage.gameObject.SetActive(false);
        }

        if (nameText) nameText.gameObject.SetActive(true);
        if (dialogueText) dialogueText.gameObject.SetActive(true);
    }

    // ===== 共通表示関数 =====
    void Narration(string text)
    {
        nameText.text = "ナレーション";
        dialogueText.text = text;
        characterLeft.enabled = false;
        characterRight.enabled = false;
    }

    void ShowLeftCharacter(string speaker, string text, bool keepRight = false)
    {
        nameText.text = speaker;
        dialogueText.text = text;
        characterLeft.enabled = true;
        if (!keepRight) characterRight.enabled = false;
    }

    void ShowRightCharacter(Sprite sprite, string speaker, string text)
    {
        nameText.text = speaker;
        dialogueText.text = text;
        characterLeft.enabled = false;
        characterRight.enabled = true;
        characterRight.sprite = sprite;
    }

    // ===== ボタン共通関数 =====
    void SetBtn(Button b, string t)
    {
        var tt = b.GetComponentInChildren<TMP_Text>();
        if (tt) tt.text = t;
    }

    void ClearChoiceListeners()
    {
        choice1Button.onClick.RemoveAllListeners();
        choice2Button.onClick.RemoveAllListeners();
        choice3Button.onClick.RemoveAllListeners();
        if (choice4Button) choice4Button.onClick.RemoveAllListeners();
    }

    void UpdateEvidencePreview(int id)
    {
        if (!evidencePreview || !evidenceCaption) return;
        switch (id)
        {
            default: evidencePreview.sprite = null; evidenceCaption.text = "提示する証拠を選んでください。"; break;
            case 1: evidencePreview.sprite = spAlarmLog; evidenceCaption.text = "証拠1：警報ログ（作動なし）"; break;
            case 2: evidencePreview.sprite = spInnerLock; evidenceCaption.text = "証拠2：内側ロック痕"; break;
            case 3: evidencePreview.sprite = spTimerDevice; evidenceCaption.text = "証拠3：タイマー式警報音装置"; break;
        }
    }

    void PlaySE(AudioClip clip)
    {
        if (sfx && clip) sfx.PlayOneShot(clip);
    }
}
