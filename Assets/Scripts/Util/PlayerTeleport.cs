using UnityEngine;

/// <summary>
/// A utility class component set onto a standalone game object
/// Simply handles teleportation for player
/// </summary>
public class PlayerTeleport : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Player Refrence")]
    [SerializeField] Transform player;

    [Header("Lobby Spawn Point")]
    [Tooltip("Where the player should be when returning back to surface")]
    [SerializeField] Transform LobbySpawnPoint;

    [Header("Gameplay Spawn Point")]
    [Tooltip("Where the player should be when  starting a run")]
    [SerializeField] Transform GameplaySpawnPoint;

    public void TeleportPlayerToLobby()
    {
        if (player != null && LobbySpawnPoint !=null)
        {
            player.position = LobbySpawnPoint.position;
        }
    }

    public void TeleportPlayerToGameplay()
    {
        if (player != null && GameplaySpawnPoint !=null)
        {
            player.position = GameplaySpawnPoint.position;
        }
    }
}
