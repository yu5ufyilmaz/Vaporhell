using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [SerializeField] private string teleportID; // Unique identifier for the teleport point

    public string TeleportID => teleportID;

    private void Reset()
    {
        // Automatically set teleportID based on GameObject name if not set
        if (string.IsNullOrEmpty(teleportID))
        {
            teleportID = gameObject.name;
        }
    }
}