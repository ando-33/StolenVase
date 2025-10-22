using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// CharacterSwitcher（探偵＝左、他キャラ＝右）
/// ナレーション → 2.5秒後に3択表示
/// </summary>
public class CharacterSwitcher : MonoBehaviour
{
    // ===== UI参照 =====
    [Header("UI")]
    public Image characterLeft;
    public Image characterRight;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Button nextButton;

    [Header("Choices Panel（序盤・終盤共用）")]
    public GameObject choicesPanel;
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
    public Image evidencePreview;
    public TMP_Text evidenceCaption;
    public TMP_Text evidenceHint;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public Button retryButton;

    // ===== Sprites =====
    [Header("Sprites")]
    public Sprite curatorNormal, curatorNervous;
    public Sprite cleanerNormal, cleanerNervous;
    public Sprite guardNormal, guardNervous;
    public Sprite directorNormal, directorConfess;
    public Sprite detectiveSprite;

    [Header("Evidence Sprites")]
    public Sprite spAlarmLog;
    public Sprite spInnerLock;
    public Sprite spTimerDevice;

    [Header("SFX (Optional)")]
    public AudioSource sfx;
    public AudioClip sfxReveal, sfxConfess;

    private enum State
    {
        Intro,
        MeetSuspects,
        SuspectSpoken,
        A_GuardTestimony,
        A_EvidenceShown,
        A_GuardNervous,
        A_After,
        B_OuterClaim,
        PreChoiceBait,
        DirectorSubtleSlip,
        CulpritChoice4,
        ExposeSlip,
        PointDirector,
        ShowDeviceEvidence,
        Confession,
        Ending,
        GameOver
    }
    private State state = State.Intro;

    private enum Interrupt { None, AlarmLog, InnerLock }
    private Interrupt interrupt = Interrupt.None;

    private bool heardCurator, heardCleaner, heardGuard;

    void Start()
    {
        // 初期UI
        choicesPanel.SetActive(false);
        evidencePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        presentButton.gameObject.SetActive(false);
        if (evidenceHint) evidenceHint.text = "";
        if (choicesHint) choicesHint.text = "";

        // 探偵を左側に表示
        if (characterLeft && detectiveSprite)
        {
            characterLeft.enabled = true;
            characterLeft.sprite = detectiveSprite;
        }

        // ボタンイベント
        retryButton.onClick.AddListener(() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));

        evAlarmBtn.onClick.AddListener(() => OnEvidence(1));
        evInnerLockBtn.onClick.AddListener(() => OnEvidence(2));
        evDeviceBtn.onClick.AddListener(() => OnEvidence(3));
        presentButton.onClick.AddListener(() => OpenEvidencePanel());

        // コルーチンでナレーション→数秒後に選択肢表示
        StartCoroutine(IntroSequence());
    }

    // --- 序盤イントロ ---
    IEnumerator IntroSequence()
    {
        if (nextButton) nextButton.gameObject.SetActive(false);
        Narration("美術館で高価な壺が“盗まれた”。まずは関係者から事情を聴こう。");
        yield return new WaitForSeconds(4.5f);
        if (nameText) nameText.text = "";
        if (dialogueText) dialogueText.text = "";
        if (nextButton) nextButton.gameObject.SetActive(true);
        ShowMeetSuspectsMenu();
    }

    // ===== 序盤：3人の供述メニュー =====
    void ShowMeetSuspectsMenu()
    {
        state = State.MeetSuspects;
        if (nameText) nameText.text = "";
        if (dialogueText) dialogueText.text = "";
        nextButton.gameObject.SetActive(false);
        choicesPanel.SetActive(true);
        if (choicesHint) choicesHint.text = "";
        if (choice4Button) choice4Button.gameObject.SetActive(false);

        SetBtn(choice1Button, "学芸員の供述を聞く");
        SetBtn(choice2Button, "清掃員の供述を聞く");
        SetBtn(choice3Button, "警備員の供述を聞く");
        ClearChoiceListeners();

        choice1Button.onClick.AddListener(() =>
        {
            choicesPanel.SetActive(false);
            nextButton.gameObject.SetActive(true);
            ShowRightCharacter(curatorNormal, "学芸員", "展示準備で手いっぱいだったわ。壺の展示室には、その時間は入っていないわ。");
            heardCurator = true;
            AfterOneSuspect();
        });

        choice2Button.onClick.AddListener(() =>
        {
            choicesPanel.SetActive(false);
            nextButton.gameObject.SetActive(true);
            ShowRightCharacter(cleanerNormal, "清掃員", "閉館後は廊下の掃除をしました。展示室の中は入っていませんよ。");
            heardCleaner = true;
            AfterOneSuspect();
        });

        choice3Button.onClick.AddListener(() =>
        {
            choicesPanel.SetActive(false);
            nextButton.gameObject.SetActive(true);
            ShowRightCharacter(guardNormal, "警備員", "“警報音を聞いて”駆け付けたときには、壺はもう無かった。窓ガラスを割って犯人は侵入したようだ。");
            heardGuard = true;
            AfterOneSuspect();
        });
    }

    void AfterOneSuspect()
    {
        state = State.SuspectSpoken;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            if (heardCurator && heardCleaner && heardGuard)
            {
                Start_A_GuardTestimony();
            }
            else
            {
                Narration("他の供述も確認しておこう。");
                ShowMeetSuspectsMenu();
            }
        });
    }

    // ===== A：警備員供述 =====
    void Start_A_GuardTestimony()
    {
        state = State.A_GuardTestimony;
        interrupt = Interrupt.AlarmLog;

        // 🔹 まず探偵（player）の問いかけを表示
        ShowLeftCharacter("探偵", "本当に警報音を聞いたのですか？");

        // 🔹 Nextボタンを押したら警備員の証言へ進む
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            ShowRightCharacter(guardNormal, "警備員", "確かに警報音を聞いたよ。“警報音を聞いた”から展示室へ急行したんだ。");
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => Show_AlarmEvidence());
        });
    }

    // 🔹 探偵が証拠を提示 → 警備員が動揺する
    void Show_AlarmEvidence()
    {
        state = State.A_EvidenceShown;
        ShowLeftCharacter("探偵", "防犯システムの記録では、該当時刻に“警報作動なし”。あなたが聞いたのは何の音ですか？");
        PlaySE(sfxReveal);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            ShowRightCharacter(guardNervous, "警備員", "な、なに？そんなはずは……！でも確かに音がしたんだ。展示室に私が着いた時には警報は止まっていたが。");
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                state = State.A_After;
                Start_B_OuterClaim();
            });
        });
    }

    // ===== B：外部犯行主張 =====
    void Start_B_OuterClaim()
    {
        state = State.B_OuterClaim;
        interrupt = Interrupt.InnerLock;
        Narration("“外から割られた”という主張。しかし現場には——");
        presentButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);
    }

    // ===== 証拠提示 =====
    void OpenEvidencePanel()
    {
        if (evidenceHint) evidenceHint.text = "";
        evidencePanel.SetActive(true);
        UpdateEvidencePreview(0);
    }

    void OnEvidence(int id)
    {
        UpdateEvidencePreview(id);
        if (interrupt == Interrupt.InnerLock)
        {
            if (id == 2)
            {
                EvidenceOK("探偵", "窓は“内側”からロック解除の痕跡。外部犯行に見せかけた偽装だ。");
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() => Start_PreChoiceBait());
            }
            else ShowEvidenceWrong("提示する証拠が違うようです。もう一度選んでください。");
        }
    }

    void ShowEvidenceWrong(string hint)
    {
        if (evidenceHint) evidenceHint.text = hint;
    }

    void EvidenceOK(string speaker, string text)
    {
        PlaySE(sfxReveal);
        evidencePanel.SetActive(false);
        presentButton.gameObject.SetActive(false);
        ShowLeftCharacter("探偵", text);
        nextButton.gameObject.SetActive(true);
    }

    // ===== 館長失言・4択 =====
    void Start_PreChoiceBait()
    {
        state = State.PreChoiceBait;
        ShowLeftCharacter("探偵", "館長、展示室で壺が盗まれた時の状況を教えてください。");
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => Director_SubtleSlip());
    }

    void Director_SubtleSlip()
    {
        state = State.DirectorSubtleSlip;
        ShowRightCharacter(directorNormal, "館長", "確かその時は盗まれた壺のあった展示台で警報音がしたのじゃ。");
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => ShowCulpritChoices4());
    }

    void ShowCulpritChoices4()
    {
        state = State.CulpritChoice4;
        if (nameText) nameText.text = "";
        if (dialogueText) dialogueText.text = "";
        nextButton.gameObject.SetActive(false);
        choicesPanel.SetActive(true);
        if (choicesHint) choicesHint.text = "";
        if (choice4Button) choice4Button.gameObject.SetActive(true);

        SetBtn(choice1Button, "学芸員");
        SetBtn(choice2Button, "清掃員");
        SetBtn(choice3Button, "警備員");
        SetBtn(choice4Button, "この中にいない");

        ClearChoiceListeners();
        choice1Button.onClick.AddListener(() => choicesHint.text = "学芸員は疑う点がない。もう一度選んでください。");
        choice2Button.onClick.AddListener(() => choicesHint.text = "清掃員は展示室に入っていない。もう一度選んでください。");
        choice3Button.onClick.AddListener(() => choicesHint.text = "警備員には決定打となる証拠がない。もう一度選んでください。");
        choice4Button.onClick.AddListener(() => Expose_SlipThenPoint());
    }

    void Expose_SlipThenPoint()
    {
        choicesPanel.SetActive(false);
        nextButton.gameObject.SetActive(true);
        state = State.ExposeSlip;
        ShowLeftCharacter("探偵", "警報音は展示台で鳴った？");
        PlaySE(sfxReveal);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => Point_Director());
    }

    void Point_Director()
    {
        state = State.PointDirector;
        ShowLeftCharacter("探偵", "真犯人は——館長、あなたです。おそらく展示台のどこかに時限式の警報装置を設置した。");
        PlaySE(sfxReveal);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => Show_DeviceEvidence());
    }

    void Show_DeviceEvidence()
    {
        state = State.ShowDeviceEvidence;
        ShowLeftCharacter("探偵", "だから館長は警報が鳴ったのが展示台だと知っていた。");
        if (evidencePreview) evidencePreview.sprite = spTimerDevice;
        if (evidenceCaption) evidenceCaption.text = "証拠3：タイマー式警報音装置（展示台裏で発見）";
        PlaySE(sfxReveal);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => Show_Confession());
    }

    void Show_Confession()
    {
        state = State.Confession;
        ShowRightCharacter(directorConfess, "館長", "……認めよう。私は数週間前に壺を割ってしまったのじゃ。それで今回、壺の展示台裏に取付た警報装置がセットした時刻に鳴るようにした。外部犯行により壺が盗まれたように見せかけたのじゃ。");
        PlaySE(sfxConfess);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            Narration("事件は解決。館長は誤って壺を割ってしまった後、贋作を展示しており、探偵が来館する日の前日深夜に贋作を隠し、警備員を犯人に仕立て上げようとしたのだった。");
            state = State.Ending;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                dialogueText.text = "（お疲れさまでした。）";
                nextButton.interactable = false;
            });
        });
    }

    // ===== 共通関数 =====
    void Narration(string text)
    {
        nameText.text = "ナレーション";
        dialogueText.text = text;
        characterLeft.enabled = false;
        characterRight.enabled = false;
    }

    void ShowLeftCharacter(string speaker, string text)
    {
        nameText.text = speaker;
        dialogueText.text = text;
        characterLeft.enabled = true;
        characterRight.enabled = false;
    }

    void ShowRightCharacter(Sprite sprite, string speaker, string text)
    {
        nameText.text = speaker;
        dialogueText.text = text;
        characterLeft.enabled = false;
        characterRight.enabled = true;
        characterRight.sprite = sprite;
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

    void PlaySE(AudioClip clip)
    {
        if (sfx && clip) sfx.PlayOneShot(clip);
    }


}
