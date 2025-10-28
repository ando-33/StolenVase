using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public Image evidencePreview;
    public TMP_Text evidenceCaption;
    public TMP_Text evidenceHint;

    [Header("Cut-in")]
    public Image cutInImage;

    [Header("Cut-in Hint")]
    public TMP_Text cutInHintText;

    [Header("Game Over UI")]
    public Button retryButton;

    [Header("Sprites")]
    public Sprite curatorNormal, cleanerNormal, guardNormal, guardNervous;
    public Sprite directorNormal, directorConfess;
    public Sprite detectiveSprite;

    [Header("Evidence Sprites")]
    public Sprite spAlarmLog;
    public Sprite spInnerLock;
    public Sprite spTimerDevice;

    [Header("SFX")]
    public AudioSource sfx;
    public AudioClip sfxReveal, sfxConfess;

    private bool heardCurator, heardCleaner, heardGuard;
    private bool isCorrectSelected = false;

    //音にまつわるコンポーネントとSE音情報
    AudioSource audio;
    public AudioClip se_present;
    public AudioClip se_click;


    void Awake()
    {
        if (!presentButton) Debug.LogError("Present Button が未割り当てです。", this);
        if (!evidencePanel) Debug.LogError("Evidence Panel が未割り当てです。", this);
    }

    void Start()
    {
        audio = GetComponent<AudioSource>();
        if (choicesPanel) choicesPanel.SetActive(false);
        if (evidencePanel) evidencePanel.SetActive(false);
        if (presentButton) presentButton.gameObject.SetActive(false);
        if (clickHintText) clickHintText.gameObject.SetActive(false);
        if (choicesTitleText) choicesTitleText.gameObject.SetActive(false);
        if (evidencePreview) evidencePreview.gameObject.SetActive(false);
        if (cutInImage) cutInImage.gameObject.SetActive(false);
        if (cutInHintText) cutInHintText.gameObject.SetActive(false);

        if (backgroundImage) backgroundImage.enabled = true;
        if (characterLeft && detectiveSprite)
        {
            characterLeft.enabled = true;
            characterLeft.sprite = detectiveSprite;
        }

        if (retryButton)
            retryButton.onClick.AddListener(() =>
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));

        if (presentButton) presentButton.onClick.AddListener(OpenEvidencePanel);
        if (evAlarmBtn) evAlarmBtn.onClick.AddListener(() => OnSelectEvidence(1));
        if (evInnerLockBtn) evInnerLockBtn.onClick.AddListener(() => OnSelectEvidence(2));
        if (evDeviceBtn) evDeviceBtn.onClick.AddListener(() => OnSelectEvidence(3));

        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        Narration("美術館で高価な壺が“盗まれた”。まずは関係者から事情を聴いて、証拠と矛盾する供述には証拠を提示しよう");
        yield return new WaitForSeconds(2f);
        yield return WaitForClick();
        ShowMeetSuspectsMenu();
    }

    IEnumerator WaitForClick()
    {
        if (clickHintText) clickHintText.gameObject.SetActive(true);
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        if (clickHintText) clickHintText.gameObject.SetActive(false);
    }

    //  会話を一時的に非表示にする
    void HideDialogue()
    {
        nameText.text = "";
        dialogueText.text = "";
    }

    void ShowMeetSuspectsMenu()
    {
        HideDialogue();
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
            StartCoroutine(SuspectDialogue("学芸員", curatorNormal, "展示準備で手いっぱいだったわ。壺の展示室には、その時間は入っていないわ。"));
            heardCurator = true;
        });

        choice2Button.onClick.AddListener(() =>
        {
            StartCoroutine(SuspectDialogue("清掃員", cleanerNormal, "閉館後は廊下の掃除をしました。展示室の中は入っていませんよ。"));
            heardCleaner = true;
        });

        choice3Button.onClick.AddListener(() =>
        {
            StartCoroutine(SuspectDialogue("警備員", guardNormal, "“警報音を聞いて”駆け付けたときには、壺はもう無かった。外から窓ガラスを割って犯人は侵入したようだ。"));
            heardGuard = true;
        });
    }

    IEnumerator SuspectDialogue(string name, Sprite img, string text)
    {
        choicesPanel.SetActive(false);
        ShowRightCharacter(img, name, text);
        yield return WaitForClick();

        if (heardCurator && heardCleaner && heardGuard)
            yield return StartCoroutine(Start_A_GuardTestimony());
        else
        {
            Narration("他の供述も確認しておこう。");
            yield return WaitForClick();
            ShowMeetSuspectsMenu();
        }
    }

    IEnumerator Start_A_GuardTestimony()
    {
        ShowLeftCharacter("探偵", "本当に警報音を聞いたのですか？", keepRight: true);
        yield return WaitForClick();

        ShowRightCharacter(guardNormal, "警備員", "確かに“警報音を聞いた”から展示室へ急行したんだ。");
        yield return WaitForClick();

        if (presentButton) presentButton.gameObject.SetActive(true);
    }

    public void OpenEvidencePanel()
    {
        HideDialogue();
        if (evidencePanel) evidencePanel.SetActive(true);
    }

    void OnSelectEvidence(int id)
    {
        if (presentButton) presentButton.gameObject.SetActive(false);

        if (id == 1 && !isCorrectSelected)
        {
            isCorrectSelected = true;
            StartCoroutine(ShowAlarmEvidenceCutIn());
        }
        else if (id == 2 && isCorrectSelected)
        {
            StartCoroutine(ShowInnerLockEvidenceCutIn());
        }
        else
        {
            StartCoroutine(WrongEvidenceFlow());
        }
    }

    //  証拠1：警報ログのカットイン
    IEnumerator ShowAlarmEvidenceCutIn()
    {
        HideDialogue();
        if (evidencePanel) evidencePanel.SetActive(false);

        if (cutInImage)
        {
            cutInImage.gameObject.SetActive(true);
            cutInImage.sprite = spAlarmLog;
            cutInImage.preserveAspect = true;
        }

        // 効果音を再生
        if (SoundManager.instance != null)
            SoundManager.instance.PlaySE(SEType.Present);

        PlaySE(sfxReveal);
        yield return WaitForClick();
        if (cutInImage) cutInImage.gameObject.SetActive(false);

        yield return StartCoroutine(CorrectEvidenceFlow());
    }

    IEnumerator CorrectEvidenceFlow()
    {
        ShowLeftCharacter("探偵", "防犯システムの記録では、該当時刻に“警報作動なし”。あなたが聞いたのは何の音ですか？");
        yield return WaitForClick();

        ShowRightCharacter(guardNervous, "警備員", "な、なに？そんなはずは……！でも確かに音がしたんだ。展示室に私が着いた時には警報は止まっていたが。");
        yield return WaitForClick();

        yield return StartCoroutine(Start_B_OuterClaim());
    }

    // 選択肢とかぶらないようセリフを非表示にする
    IEnumerator WrongEvidenceFlow()
    {
        if (evidencePanel) evidencePanel.SetActive(false);
        ShowLeftCharacter("探偵", "提示する証拠が違うようだ。もう一度選び直そう。");
        yield return WaitForClick();

        HideDialogue(); // ここで発言を消してから証拠パネルを再表示
        if (evidencePanel) evidencePanel.SetActive(true);
    }

    IEnumerator Start_B_OuterClaim()
    {
        Narration("“外から窓ガラスを割って犯人は侵入したようだ”という主張。しかし現場には——");
        yield return WaitForClick();

        if (presentButton) presentButton.gameObject.SetActive(true);
        yield return new WaitUntil(() => evidencePanel.activeSelf);
    }

    //  証拠2：内側ロックのカットイン
    IEnumerator ShowInnerLockEvidenceCutIn()
    {
        HideDialogue();
        if (evidencePanel) evidencePanel.SetActive(false);

        if (cutInImage)
        {
            cutInImage.gameObject.SetActive(true);
            cutInImage.sprite = spInnerLock;
            cutInImage.preserveAspect = true;
        }

        // 効果音を再生
        if (SoundManager.instance != null)
            SoundManager.instance.PlaySE(SEType.Present);

        PlaySE(sfxReveal);
        yield return WaitForClick();
        if (cutInImage) cutInImage.gameObject.SetActive(false);

        EvidenceOK("探偵", "窓は“内側”からロックが解除されていた。外部犯行に見せかけた偽装だ。");
        yield return WaitForClick();

        yield return StartCoroutine(Start_PreChoiceBait());
    }

    void EvidenceOK(string speaker, string text)
    {
        ShowLeftCharacter(speaker, text);
    }

    IEnumerator Start_PreChoiceBait()
    {
        ShowLeftCharacter("探偵", "館長、展示室で壺が盗まれた時の状況を教えてください。");
        yield return WaitForClick();

        ShowRightCharacter(directorNormal, "館長", "警備員に呼ばれて展示室に向かうと、展示台で鳴った警報は止まっておった。");
        yield return WaitForClick();

        ShowCulpritChoices4();
    }

    void ShowCulpritChoices4()
    {
        HideDialogue();
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
            // 効果音を再生
            AudioSource.PlayClipAtPoint(se_click, Camera.main.transform.position);

            // クリックした瞬間にBGMをエンディングに切り替える
            if (SoundManager.instance != null)
            {
                SoundManager.instance.StopBgm();           // 現在のBGMを止める
                if (BGMManagerExists())
                {
                    FindObjectOfType<BGMManager>().PlayEndingBGM();
                }
            }

            choicesPanel.SetActive(false);
            if (choicesTitleText) choicesTitleText.gameObject.SetActive(false);
            StartCoroutine(Show_Confession());
        });


    }

    IEnumerator Show_Confession()
    {
        ShowLeftCharacter("探偵", "真犯人は館長、あなたです。あなたは警報音が展示“台”から鳴っていたことを知っていた。展示台の裏に仕掛けられた“タイマー式警報装置”がその証拠です。");
        PlaySE(sfxReveal);
        yield return WaitForClick();

        ShowRightCharacter(directorConfess, "館長", "……認めよう。窓の内側からロックを外した上で窓ガラスを割った。そしてタイマーで鳴る警報装置を鳴らせることで外部犯行に見せかけたのじゃ。");
        PlaySE(sfxConfess);
        yield return WaitForClick();

        Narration("事件は解決。高価な壺は盗難保険を掛けられていた。館長は壺が何者かに盗まれたとして保険金を騙し取るつもりだったのだ。");
        yield return WaitForClick();

        dialogueText.text = "（お疲れさまでした）";
    }

    bool BGMManagerExists()
    {
        return FindObjectOfType<BGMManager>() != null;
    }


    // 共通関数
    void Narration(string text)
    {
        nameText.text = "";
        dialogueText.text = text;
        if (characterLeft) characterLeft.enabled = false;
        if (characterRight) characterRight.enabled = false;
    }

    void ShowLeftCharacter(string speaker, string text, bool keepRight = false)
    {
        nameText.text = speaker;
        dialogueText.text = text;
        if (characterLeft) characterLeft.enabled = true;
        if (!keepRight && characterRight) characterRight.enabled = false;
    }

    void ShowRightCharacter(Sprite sprite, string speaker, string text)
    {
        nameText.text = speaker;
        dialogueText.text = text;
        if (characterLeft) characterLeft.enabled = false;
        if (characterRight)
        {
            characterRight.enabled = true;
            characterRight.sprite = sprite;
        }
    }

    void SetBtn(Button b, string t)
    {
        if (!b) return;
        var tt = b.GetComponentInChildren<TMP_Text>();
        if (tt) tt.text = t;
    }

    void ClearChoiceListeners()
    {
        if (choice1Button) choice1Button.onClick.RemoveAllListeners();
        if (choice2Button) choice2Button.onClick.RemoveAllListeners();
        if (choice3Button) choice3Button.onClick.RemoveAllListeners();
        if (choice4Button) choice4Button.onClick.RemoveAllListeners();
    }

    void PlaySE(AudioClip clip)
    {
        if (sfx && clip) sfx.PlayOneShot(clip);
    }
}
