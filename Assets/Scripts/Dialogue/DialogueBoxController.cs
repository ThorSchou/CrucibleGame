using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueBoxController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioSource dialogueAudioSource;
    [SerializeField] private Dialogue dialogue;
    private DialogueTrigger currentDialogueTrigger;
    private GameObject finishTalkingActivateGameObject;

    [Header("Input")]
    [SerializeField] private InputActionReference submitAction;
    [SerializeField] private InputActionReference horizontalAction;

    [Header("Sounds")]
    private AudioClip[] audioLines;
    private AudioClip[] audioChoices;
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip[] typeSounds;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI choice1Mesh;
    [SerializeField] private TextMeshProUGUI choice2Mesh;
    [SerializeField] private TextMeshProUGUI nameMesh;
    [SerializeField] private TextMeshProUGUI textMesh;

    [Header("Settings")]
    [SerializeField] private float typeSpeed = 1f;

    private bool ableToAdvance;
    private bool activated;
    private int choiceLocation;
    private int cPos = 0;
    private string[] characterDialogue;
    private string[] choiceDialogue;
    private DialogueTrigger dialogueTrigger;
    [System.NonSerialized] public bool extendConvo;
    private string finishTalkingAnimatorBool;
    private string finishTalkingActivateGameObjectString;
    private string fileName;
    private int index = -1;
    private bool repeat;
    private bool typing = true;
    private bool pendingSubmit = false;
    private bool horizontalWasDown;

    private void OnEnable()
    {
        if (submitAction != null) submitAction.action.Enable();
        if (horizontalAction != null) horizontalAction.action.Enable();
    }

    private void OnDisable()
    {
        if (submitAction != null) submitAction.action.Disable();
        if (horizontalAction != null) horizontalAction.action.Disable();
    }

    void Update()
    {
        if (submitAction != null && submitAction.action.WasPressedThisFrame())
            pendingSubmit = true;

        if (!activated) return;

        HandleSubmit();
        HandleHorizontal();
    }

    private void HandleSubmit()
    {
        if (!pendingSubmit || typing) return;
        pendingSubmit = false;

        if (index < choiceLocation || (extendConvo && index < characterDialogue.Length - 1))
        {
            if (ableToAdvance) StartCoroutine(Advance());
        }
        else
        {
            StartCoroutine(Close());
        }
    }

    private void HandleHorizontal()
    {
        float horizontal = horizontalAction != null ? horizontalAction.action.ReadValue<float>() : 0f;
        bool hasChoices = animator.GetBool("hasChoices");

        if (horizontal != 0f && !horizontalWasDown && hasChoices)
        {
            int current = animator.GetInteger("choiceSelection");
            if (current == 1)
            {
                animator.SetInteger("choiceSelection", 2);
                extendConvo = true;
            }
            else
            {
                animator.SetInteger("choiceSelection", 1);
                extendConvo = false;
            }
            if (selectionSound != null) audioSource.PlayOneShot(selectionSound);
            horizontalWasDown = true;
        }

        if (horizontal == 0f) horizontalWasDown = false;
    }

    public void Appear(string fName, string characterName, DialogueTrigger dTrigger,
        bool useItemAfterClose, AudioClip[] audioL, AudioClip[] audioC,
        string finishTalkingAnimBool, GameObject finishTalkingActivateGObject,
        string finishTalkingActivateGOString, bool r)
    {
        repeat = r;
        finishTalkingAnimatorBool = finishTalkingAnimBool;
        finishTalkingActivateGameObject = finishTalkingActivateGObject;
        finishTalkingActivateGameObjectString = finishTalkingActivateGOString;
        dialogueTrigger = dTrigger;
        fileName = fName;
        audioLines = audioL;
        audioChoices = audioC;

        choice1Mesh.text = "";
        choice2Mesh.text = "";

        if (useItemAfterClose) currentDialogueTrigger = dialogueTrigger;

        nameMesh.text = characterName;
        characterDialogue = dialogue.dialogue[fileName];

        if (dialogue.dialogue.ContainsKey(fileName + "Choice1"))
        {
            choiceDialogue = dialogue.dialogue[fileName + "Choice1"];
            choiceLocation = GetChoiceLocation();
        }
        else
        {
            choiceLocation = characterDialogue.Length - 1;
        }

        animator.SetBool("active", true);
        activated = true;
        ableToAdvance = true;
        pendingSubmit = false;
        NewPlayer.Instance.Freeze(true);
        StartCoroutine(Advance());
    }

    IEnumerator Close()
    {
        if (index == choiceLocation
            && dialogue.dialogue.ContainsKey(fileName + "Choice1")
            && audioChoices.Length != 0)
        {
            audioSource.Stop();
            yield return new WaitForSeconds(.1f);
            int choice = animator.GetInteger("choiceSelection");
            dialogueAudioSource.PlayOneShot(audioChoices[choice == 1 ? 0 : 1]);
        }

        if (currentDialogueTrigger != null) currentDialogueTrigger.UseItem();

        activated = false;
        index = -1;
        pendingSubmit = false;
        ableToAdvance = false;
        extendConvo = false;
        choiceLocation = 0;

        animator.SetBool("active", false);
        StopCoroutine("TypeText");
        ShowChoices(false);

        if (!string.IsNullOrEmpty(finishTalkingAnimatorBool))
            dialogueTrigger.useItemAnimator.SetBool(finishTalkingAnimatorBool, true);

        int finalChoice = animator.GetInteger("choiceSelection");
        if (finishTalkingActivateGameObject != null && finalChoice == 1)
            finishTalkingActivateGameObject.SetActive(true);
        else if (!string.IsNullOrEmpty(finishTalkingActivateGameObjectString) && finalChoice == 1)
            GameObject.Find(finishTalkingActivateGameObjectString)
                      .GetComponent<BoxCollider2D>().enabled = true;

        if (!repeat) dialogueTrigger.completed = true;

        dialogueTrigger = null;
        currentDialogueTrigger = null;
        finishTalkingAnimatorBool = "";
        finishTalkingActivateGameObject = null;
        finishTalkingActivateGameObjectString = "";

        yield return new WaitForSeconds(1f);
        NewPlayer.Instance.Freeze(false);
        animator.SetInteger("choiceSelection", 1);
    }

    IEnumerator Advance()
    {
        index++;
        typing = true;

        if (ableToAdvance) animator.SetTrigger("press");
        if (index != choiceLocation) ShowChoices(false);

        if (index == choiceLocation + 1
            && dialogue.dialogue.ContainsKey(fileName + "Choice1")
            && audioChoices.Length != 0)
        {
            audioSource.Stop();
            yield return new WaitForSeconds(.1f);
            int choice = animator.GetInteger("choiceSelection");
            AudioClip clip = audioChoices[choice == 1 ? 0 : 1];
            dialogueAudioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }

        textMesh.text = "";
        StartCoroutine("TypeText");
        yield return new WaitForSeconds(.4f);

        if (index == choiceLocation && dialogue.dialogue.ContainsKey(fileName + "Choice1"))
            ShowChoices(true);

        if (audioLines.Length != 0 && index < audioLines.Length && audioLines[index] != null)
        {
            dialogueAudioSource.Stop();
            dialogueAudioSource.PlayOneShot(audioLines[index]);
        }
    }

    IEnumerator TypeText()
    {
        WaitForSeconds wait = new WaitForSeconds(.01f / typeSpeed);
        foreach (char c in characterDialogue[index])
        {
            cPos++;
            if (cPos == characterDialogue[index].Length)
            {
                typing = false;
                cPos = 0;
            }
            textMesh.text += c;
            if (typeSounds != null && typeSounds.Length > 0)
                audioSource.PlayOneShot(typeSounds[Random.Range(0, typeSounds.Length)],
                                        Random.Range(.3f, .5f));
            yield return wait;
        }
    }

    public int GetChoiceLocation()
    {
        for (int i = 0; i < choiceDialogue.Length; i++)
            if (choiceDialogue[i] != "") return i;
        return 0;
    }

    void ShowChoices(bool show)
    {
        animator.SetBool("hasChoices", show);
        if (!show) return;
        choice1Mesh.text = dialogue.dialogue[fileName + "Choice1"][choiceLocation];
        choice2Mesh.text = dialogue.dialogue[fileName + "Choice2"][choiceLocation];
    }
}