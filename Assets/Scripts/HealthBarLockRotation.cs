using UnityEngine;

public class HealthBarLockRotation : MonoBehaviour
{
    private Transform parentTransform;   // Sağlık çubuğunun parent'ı (düşman karakter)
    private Vector3 initialLocalPosition; // Sağlık çubuğunun başlangıç yerel pozisyonu

    void Start()
    {
        // Sağlık çubuğunun bağlı olduğu parent'ı al
        parentTransform = transform.parent;

        // Başlangıç yerel pozisyonunu kaydet
        initialLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (parentTransform != null)
        {
            // Sağlık çubuğunun pozisyonunu parent ile eşitle
            transform.position = parentTransform.position + initialLocalPosition;

            // Parent'ın döndüğüne göre sağlık çubuğunu düzelt
            if (parentTransform.localScale.x < 0)
            {
                // Parent ters dönmüşse, sağlık çubuğunu ters çevir
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                // Normal pozisyonda ise sağlık çubuğu düz kalsın
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}