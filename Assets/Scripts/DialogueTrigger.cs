
using System.Collections.Generic;
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

    // Track all colliders belonging to each player object.
    private readonly HashSet<PhotonView> lowerHalves =
        new HashSet<PhotonView>();

    private readonly HashSet<PhotonView> upperHalves =
        new HashSet<PhotonView>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        PhotonView view = other.GetComponentInParent<PhotonView>();

        if (view == null)
            return;

        if (view.GetComponent<LowerHalfController>() != null)
            lowerHalves.Add(view);

        if (view.GetComponent<UpperHalfController>() != null)
            upperHalves.Add(view);

        CheckBothHalvesInside();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PhotonView view = other.GetComponentInParent<PhotonView>();

        if (view == null)
            return;

        lowerHalves.Remove(view);
        upperHalves.Remove(view);
    }

    private void CheckBothHalvesInside()
    {
        if (lowerHalves.Count == 0 || upperHalves.Count == 0)
            return;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager is missing!");
            return;
        }

        if (DialogueManager.Instance.IsPlaying)
            return;

        if (triggerOnce)
            hasTriggered = true;

        DialogueManager.Instance.StartDialogue(
            speakers,
            dialogueLines
        );
    }
}
