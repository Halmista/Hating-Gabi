
using Photon.Pun;
using UnityEngine;

public class LevelSpawnManager : MonoBehaviour
{
    public Transform lowerHalfSpawn;
    public Transform upperHalfSpawn;

    private void Start()
    {
        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        if (!PhotonNetwork.InRoom)
            return;

        Vector3 spawnPosition;
        string prefabName;

        // Actor 1 plays the Lower Half; Actor 2 plays the Upper Half.
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            prefabName = "LowerHalf";
            spawnPosition = lowerHalfSpawn.position;
        }
        else
        {
            prefabName = "UpperHalf";
            spawnPosition = upperHalfSpawn.position;
        }

        PhotonNetwork.Instantiate(
            prefabName,
            spawnPosition,
            Quaternion.identity
        );
    }
}
