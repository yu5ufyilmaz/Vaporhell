using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndependentParallax : MonoBehaviour
{
    float length, startpos;
    public GameObject cam; // Kamera referansı
    public float parallaxEffectX; // Parallax efektinin şiddeti

    public float moveSpeed = 2f; // Objeyi kendiliğinden hareket ettirme hızı
    private float startY; // Y eksenini sabitlemek için başlangıç pozisyonu

    void Start()
    {
        startpos = transform.position.x; // Objenin başlangıç X pozisyonu
        startY = transform.position.y; // Objenin başlangıç Y pozisyonu
        length = GetComponent<SpriteRenderer>().bounds.size.x; // Objenin genişliği
    }

    void FixedUpdate()
    {
        // Objenin kendi kendine hareket etmesini sağla
        startpos -= moveSpeed * Time.fixedDeltaTime;

        // Kamera pozisyonuna göre geçici pozisyon hesaplaması
        float temp = (cam.transform.position.x * (1 - parallaxEffectX));
        float dist = (cam.transform.position.x * parallaxEffectX);

        // Obje pozisyonunu güncelle (kameranın etkisi + kendi hareketi)
        transform.position = new Vector3(startpos + dist, startY, transform.position.z);

        // Sonsuz kaydırma mantığı
        if (temp > startpos + length) 
            startpos += length;
        else if (temp < startpos - length) 
            startpos -= length;
    }
}