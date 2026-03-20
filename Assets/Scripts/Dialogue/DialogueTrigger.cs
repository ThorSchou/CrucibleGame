using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject finishTalkingActivateObject;
    [SerializeField] private Animator iconAnimator;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Trigger")]
    [SerializeField] private bool autoHit;
    public bool completed;
    [SerializeField] private bool repeat;
    [SerializeField] private bool sleeping;

    [Header("Dialogue")]
    [SerializeField] private string characterName;
    [SerializeField] private string dialogueStringA;
    [SerializeField] private string dialogueStringB;
    [SerializeField] private AudioClip[] audioLinesA;
    [SerializeField] private AudioClip[] audioLinesB;
    [SerializeField] private AudioClip[] audioChoices;

    [Header("Fetch Quest")]
    [SerializeField] private GameObject deleteGameObject;
    [SerializeField] private string getWhichItem;
    [SerializeField] private int getCoinAmount;
    [SerializeField] private string finishTalkingAnimatorBool;
    [SerializeField] private string finishTalkingActivateObjectString;
    [SerializeField] private Sprite getItemSprite;
    [SerializeField] private AudioClip getSound;
    [SerializeField] private bool instantGet;
    [SerializeField] private string requiredItem;
    [SerializeField] private int requiredCoins;
    public Animator useItemAnimator;
    [SerializeField] private string useItemAnimatorBool;

    private bool pendingInteract = false;

    private void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.action.Disable();
    }

    void Update()
    {
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
            pendingInteract = true;
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (instantGet) InstantGet();

        if (col.gameObject != NewPlayer.Instance.gameObject || sleeping || completed || !NewPlayer.Instance.grounded)
        {
            iconAnimator.SetBool("active", false);
            pendingInteract = false;
            return;
        }

        iconAnimator.SetBool("active", true);

        bool interactPressedThisFrame = autoHit || pendingInteract;
        pendingInteract = false;

        if (!interactPressedThisFrame) return;

        iconAnimator.SetBool("active", false);

        // Determine which dialogue string to show based on whether requirements are met
        bool requirementsMet = (requiredItem != "" && GameManager.Instance.inventory.ContainsKey(requiredItem))
                            || (requiredCoins != 0 && GameManager.Instance.coins >= requiredCoins);

        bool noRequirements = requiredItem == "" && requiredCoins == 0;

        if (noRequirements || !requirementsMet)
        {
            GameManager.Instance.dialogueBoxController.Appear(
                dialogueStringA, characterName, this, false,
                audioLinesA, audioChoices, finishTalkingAnimatorBool,
                finishTalkingActivateObject, finishTalkingActivateObjectString, repeat);
        }
        else
        {
            if (dialogueStringB != "")
            {
                GameManager.Instance.dialogueBoxController.Appear(
                    dialogueStringB, characterName, this, true,
                    audioLinesB, audioChoices, "", null, "", repeat);
            }
            else
            {
                UseItem();
            }
        }

        sleeping = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject != NewPlayer.Instance.gameObject) return;
        iconAnimator.SetBool("active", false);
        sleeping = completed;
    }

    public void UseItem()
    {
        if (completed) return;

        if (!string.IsNullOrEmpty(useItemAnimatorBool))
            useItemAnimator.SetBool(useItemAnimatorBool, true);

        if (deleteGameObject != null)
            Destroy(deleteGameObject);

        Collect();

        if (GameManager.Instance.inventory.ContainsKey(requiredItem))
            GameManager.Instance.RemoveInventoryItem(requiredItem);
        else if (requiredCoins > 0)
            GameManager.Instance.SpendCoins(requiredCoins);

        repeat = false;
    }

    public void Collect()
    {
        if (completed) return;

        if (!string.IsNullOrEmpty(getWhichItem))
            GameManager.Instance.GetInventoryItem(getWhichItem, getItemSprite);

        if (getCoinAmount != 0)
            GameManager.Instance.AddCoins(getCoinAmount);

        if (getSound != null)
            GameManager.Instance.audioSource.PlayOneShot(getSound);

        completed = true;
    }

    public void InstantGet()
    {
        GameManager.Instance.GetInventoryItem(getWhichItem, null);
        instantGet = false;
    }
}