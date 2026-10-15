
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class LevelExitDoor : MonoBehaviourPun
{
    [Header("Exit Settings")]
    public string nextSceneName = "Level2";
    public float exitRange = 1.5f;

    [Header("Optional Door Visuals")]
    public SpriteRenderer[] doorSprites;
    public Collider2D[] doorColliders;

    private bool isUnlocked;
    private bool transitionStarted;

    private void Start()
    {
        // Door 2 starts inaccessible.
        ApplyDoorState();
    }

    public void SetUnlocked(bool unlocked)
    {
        photonView.RPC(
            nameof(RPC_SetUnlocked),
            RpcTarget.All,
            unlocked
        );
    }

    [PunRPC]
    private void RPC_SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        ApplyDoorState();

        Debug.Log(
            isUnlocked
                ? "Exit door is now unlocked."
                : "Exit door is locked."
        );
    }

    private void ApplyDoorState()
    {
        // Keep Door 2 visible, but disable its collision until unlocked.
        if (doorSprites != null)
        {
            foreach (SpriteRenderer sprite in doorSprites)
            {
                if (sprite != null)
                    sprite.enabled = true;
            }
        }

        if (doorColliders != null)
        {
            foreach (Collider2D col in doorColliders)
            {
                if (col != null)
                    col.enabled = isUnlocked;
            }
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient ||
            !isUnlocked ||
            transitionStarted)
        {
            return;
        }

        if (PhotonNetwork.PlayerList.Length < 2)
            return;

        if (BothPlayersInRange())
        {
            transitionStarted = true;

            if (!PhotonNetwork.AutomaticallySyncScene)
                PhotonNetwork.AutomaticallySyncScene = true;

            PhotonNetwork.LoadLevel(nextSceneName);
        }
    }

    private bool BothPlayersInRange()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            bool foundCharacter = false;
            bool playerIsNearExit = false;

            PhotonView[] views =
                FindObjectsByType<PhotonView>(
                    FindObjectsSortMode.None
                );

            foreach (PhotonView view in views)
            {
                if (view == null ||
                    view.OwnerActorNr != player.ActorNumber)
                {
                    continue;
                }

                bool isPlayerCharacter =
                    view.GetComponent<LowerHalfController>() != null ||
                    view.GetComponent<UpperHalfController>() != null;

                if (!isPlayerCharacter)
                    continue;

                foundCharacter = true;

                float distance = Vector2.Distance(
                    transform.position,
                    view.transform.position
                );

                if (distance <= exitRange)
                    playerIsNearExit = true;

                break;
            }

            // Every connected player must have a character
            // and that character must be near Door 2.
            if (!foundCharacter || !playerIsNearExit)
                return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, exitRange);
    }
}
