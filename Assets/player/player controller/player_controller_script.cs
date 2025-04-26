using UnityEngine;
using UnityEngine.InputSystem;

public class DebugJoin : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        Debug.Log($"Player Joined! Device: {player.devices[0].displayName}");
    }
}
