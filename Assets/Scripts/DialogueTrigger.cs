
using UnityEngine;
using Photon.Pun;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public string[] speakers;
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Trigger Settings")]
    public bool triggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        PhotonView playerView =
            other.GetComponentInParent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager is missing!");
            return;
        }

        if (DialogueManager.Instance.IsPlaying)
            return;

        hasTriggered = true;

        DialogueManager.Instance.StartDialogue(
            speakers,
            dialogueLines
        );
    }
}
