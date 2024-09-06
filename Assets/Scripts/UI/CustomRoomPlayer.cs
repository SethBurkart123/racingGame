using Mirror;
using UnityEngine;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    private PlayerListUI playerListUI;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Find the UI object in the scene and call AddPlayer
        playerListUI = FindObjectOfType<PlayerListUI>();
        if (playerListUI != null)
        {
            playerListUI.AddPlayer(this);
        }
    }

    public override void OnStopClient()
    {
        // When the player disconnects, remove them from the UI
        if (playerListUI != null)
        {
            playerListUI.RemovePlayer(this);
        }
        
        base.OnStopClient();
    }
}
