using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    float length, startpos;
    public GameObject cam;
    public float parallaxEffectX;

    private float startY; // Y eksenini sabitlemek için başlangıç pozisyonu

    void Start()
    {
        startpos = transform.position.x;
        startY = transform.position.y; // Y eksenindeki başlangıç pozisyonunu kaydediyoruz
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        // Parallax effect for X-axis
        float temp = (cam.transform.position.x * (1 - parallaxEffectX));
        float dist = (cam.transform.position.x * parallaxEffectX);

        // Update position
        transform.position = new Vector3(startpos + dist, startY, transform.position.z);

        // Infinite scrolling logic for X-axis
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
}