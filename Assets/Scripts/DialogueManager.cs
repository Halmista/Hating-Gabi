
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

public class DialogueManager : MonoBehaviourPun
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;

    private string[] lines;
    private string[] speakers;
    private int currentLine;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying || Keyboard.current == null)
            return;

        // Only the local Upper Half player can advance.
        if (!IsLocalUpperHalfPlayer())
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            photonView.RPC(
                nameof(RPC_RequestNextLine),
                RpcTarget.MasterClient
            );
        }
    }

    private bool IsLocalUpperHalfPlayer()
    {
        UpperHalfController[] upperHalves =
            FindObjectsByType<UpperHalfController>(
                FindObjectsSortMode.None
            );

        foreach (UpperHalfController upper in upperHalves)
        {
            PhotonView view = upper.GetComponent<PhotonView>();

            if (view != null && view.IsMine)
                return true;
        }

        return false;
    }

    // Call this from your existing dialogue trigger.
    public void StartDialogue(
        string[] newSpeakers,
        string[] newLines)
    {
        if (newLines == null || newLines.Length == 0)
            return;

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning(
                "Dialogue synchronization requires a Photon room."
            );
            return;
        }

        photonView.RPC(
            nameof(RPC_RequestStartDialogue),
            RpcTarget.MasterClient,
            newSpeakers,
            newLines
        );
    }

    [PunRPC]
    private void RPC_RequestStartDialogue(
        string[] newSpeakers,
        string[] newLines,
        PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient ||
            isPlaying ||
            newLines == null ||
            newLines.Length == 0)
        {
            return;
        }

        // Master Client decides when the dialogue starts.
        photonView.RPC(
            nameof(RPC_BeginDialogue),
            RpcTarget.All,
            newSpeakers,
            newLines
        );
    }

    [PunRPC]
    private void RPC_BeginDialogue(
        string[] newSpeakers,
        string[] newLines)
    {
        if (isPlaying || newLines == null || newLines.Length == 0)
            return;

        lines = newLines;
        speakers = newSpeakers;
        currentLine = 0;
        isPlaying = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        DisplayCurrentLine();
    }

    [PunRPC]
    private void RPC_RequestNextLine(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient || !isPlaying)
            return;

        // Only Actor 2, the Upper Half player, may advance.
        if (info.Sender == null || info.Sender.ActorNumber != 2)
            return;

        int nextLine = currentLine + 1;

        if (nextLine >= lines.Length)
        {
            photonView.RPC(
                nameof(RPC_EndDialogue),
                RpcTarget.All
            );
        }
        else
        {
            photonView.RPC(
                nameof(RPC_DisplayLine),
                RpcTarget.All,
                nextLine
            );
        }
    }

    [PunRPC]
    private void RPC_DisplayLine(int lineIndex)
    {
        if (!isPlaying || lines == null ||
            lineIndex < 0 || lineIndex >= lines.Length)
        {
            return;
        }

        currentLine = lineIndex;
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (dialogueText != null)
            dialogueText.text = lines[currentLine];

        if (speakerText != null)
        {
            speakerText.text =
                speakers != null && currentLine < speakers.Length
                    ? speakers[currentLine]
                    : "";
        }
    }

    [PunRPC]
    private void RPC_EndDialogue()
    {
        isPlaying = false;
        lines = null;
        speakers = null;
        currentLine = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void EndDialogue()
    {
        // Route manual requests through the Master Client too.
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(
                    nameof(RPC_EndDialogue),
                    RpcTarget.All
                );
            }
            else
            {
                photonView.RPC(
                    nameof(RPC_RequestEndDialogue),
                    RpcTarget.MasterClient
                );
            }
        }
        else
        {
            RPC_EndDialogue();
        }
    }

    [PunRPC]
    private void RPC_RequestEndDialogue()
    {
        if (!PhotonNetwork.IsMasterClient || !isPlaying)
            return;

        photonView.RPC(
            nameof(RPC_EndDialogue),
            RpcTarget.All
        );
    }
}
