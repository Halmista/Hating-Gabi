
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Doors")]
    public List<PressurePlateDoor> targetDoors =
        new List<PressurePlateDoor>();

    [Header("Animation")]
    public PressurePlateAnimation plateAnimation;

    [Header("Projectile")]
    public string projectilePrefabName = "PressurePlateProjectile";
    public Transform projectileSpawnPoint;
    public float projectileDelay = 2.5f;
    public Vector2 projectileDirection = Vector2.left;

    [Header("Respawn")]
    public Transform lowerHalfRespawnPoint;

    private readonly HashSet<Collider2D> lowerHalfColliders =
        new HashSet<Collider2D>();

    private Coroutine projectileCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        LowerHalfController lower =
            other.GetComponentInParent<LowerHalfController>();

        if (lower == null)
            return;

        PhotonView playerView = lower.GetComponent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        bool wasEmpty = lowerHalfColliders.Count == 0;
        lowerHalfColliders.Add(other);

        if (wasEmpty && lowerHalfColliders.Count > 0)
        {
            if (plateAnimation != null)
                plateAnimation.SetPressed(true);

            SetAllDoors(true);

            if (projectileCoroutine == null)
            {
                projectileCoroutine =
                    StartCoroutine(FireProjectileAfterDelay(lower));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LowerHalfController lower =
            other.GetComponentInParent<LowerHalfController>();

        if (lower == null)
            return;

        PhotonView playerView = lower.GetComponent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        lowerHalfColliders.Remove(other);

        if (lowerHalfColliders.Count == 0)
        {
            if (plateAnimation != null)
                plateAnimation.SetPressed(false);

            SetAllDoors(false);

            // The projectile still fires if the player leaves the plate.
            // The coroutine reference is cleared when it finishes.
        }
    }

    private IEnumerator FireProjectileAfterDelay(
        LowerHalfController lower)
    {
        yield return new WaitForSeconds(projectileDelay);

        if (lower != null && PhotonNetwork.InRoom)
        {
            Vector2 spawnPosition = projectileSpawnPoint != null
                ? (Vector2)projectileSpawnPoint.position
                : (Vector2)transform.position;

            float horizontalDirection =
                projectileDirection.x >= 0f ? 1f : -1f;

            Vector2 direction =
                new Vector2(horizontalDirection, 0f);

            GameObject projectile = PhotonNetwork.Instantiate(
                projectilePrefabName,
                spawnPosition,
                Quaternion.identity
            );

            PressurePlateProjectile projectileScript =
                projectile.GetComponent<PressurePlateProjectile>();

            if (projectileScript != null)
            {
                Vector2 respawnPosition = lowerHalfRespawnPoint != null
                    ? (Vector2)lowerHalfRespawnPoint.position
                    : Vector2.zero;

                bool hasRespawnPoint =
                    lowerHalfRespawnPoint != null;

                projectileScript.Initialize(
                    direction,
                    respawnPosition,
                    hasRespawnPoint
                );
            }
            else
            {
                Debug.LogError(
                    "PressurePlateProjectile component is missing!"
                );
            }
        }

        projectileCoroutine = null;
    }

    private void SetAllDoors(bool open)
    {
        foreach (PressurePlateDoor door in targetDoors)
        {
            if (door != null)
                door.SetDoorOpen(open);
        }
    }
}
