
using Photon.Pun;
using UnityEngine;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform lowerHalfSpawn;
    public Transform upperHalfSpawn;

    [Header("Fallback Positions")]
    public Vector2 lowerSpawnPosition = new Vector2(-2f, -1.5f);
    public Vector2 upperSpawnPosition = new Vector2(2f, 0f);

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError(
                "Cannot spawn players: not in a Photon room."
            );
            return;
        }

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        Vector3 spawnPosition;
        string prefabName;

        if (actorNumber == 1)
        {
            prefabName = "LowerHalf";

            spawnPosition = lowerHalfSpawn != null
                ? lowerHalfSpawn.position
                : lowerSpawnPosition;

            Debug.Log("Spawning Lower Half for Player 1.");
        }
        else if (actorNumber == 2)
        {
            prefabName = "UpperHalf";

            spawnPosition = upperHalfSpawn != null
                ? upperHalfSpawn.position
                : upperSpawnPosition;

            Debug.Log("Spawning Upper Half for Player 2.");
        }
        else
        {
            Debug.LogWarning(
                "This prototype supports only two players."
            );
            return;
        }

        PhotonNetwork.Instantiate(
            prefabName,
            spawnPosition,
            Quaternion.identity
        );
    }
}
