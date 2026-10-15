
using UnityEngine;
using Photon.Pun;

public class TutorialTrigger : MonoBehaviour
{
    public enum CharacterType
    {
        LowerHalf,
        UpperHalf
    }

    [Header("Tutorial Settings")]
    public CharacterType characterType;

    [TextArea(2, 4)]
    public string message;

    public TutorialMessage tutorialMessage;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        PhotonView playerView =
            other.GetComponentInParent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        bool correctCharacter = false;

        if (characterType == CharacterType.LowerHalf)
        {
            correctCharacter =
                other.GetComponentInParent<LowerHalfController>() != null;
        }
        else if (characterType == CharacterType.UpperHalf)
        {
            correctCharacter =
                other.GetComponentInParent<UpperHalfController>() != null;
        }

        if (!correctCharacter)
            return;

        triggered = true;

        if (tutorialMessage != null)
        {
            tutorialMessage.ShowMessage(message);
        }
    }
}
