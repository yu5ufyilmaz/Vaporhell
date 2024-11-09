using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // Takip edilecek oyuncu

    void LateUpdate()
    {
        // Oyuncunun pozisyonunu al ve Z eksenini sabit tut
        Vector3 newPosition = player.position;
        newPosition.z = transform.position.z; // Kamera yüksekliğini sabit tut
        transform.position = newPosition;
    }
}
