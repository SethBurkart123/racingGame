using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerListUI : MonoBehaviour
{
    public GameObject playerListEntryPrefab;  // Prefab for the player entry
    public Transform playerListContainer;     // The container that holds the player entries

    // Change the dictionary key type to uint
    private Dictionary<uint, GameObject> playerEntries = new Dictionary<uint, GameObject>();

    // Called when a player joins the room
    public void AddPlayer(NetworkRoomPlayer player)
    {
        // Create a new entry in the UI
        GameObject entry = Instantiate(playerListEntryPrefab, playerListContainer);
        // Convert uint to string for display
        entry.GetComponent<TMPro.TextMeshProUGUI>().text = "Player " + player.netId.ToString();

        // Store the entry by player ID so we can remove or update it later
        playerEntries[player.netId] = entry;
    }

    // Called when a player leaves the room
    public void RemovePlayer(NetworkRoomPlayer player)
    {
        if (playerEntries.ContainsKey(player.netId))
        {
            Destroy(playerEntries[player.netId]);
            playerEntries.Remove(player.netId);
        }
    }
}