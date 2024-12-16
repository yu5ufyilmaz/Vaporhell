using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    float length, startpos;
    public GameObject cam;
    public float parallaxEffectX;

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        // Parallax effect for X-axis
        float temp = (cam.transform.position.x * (1 - parallaxEffectX));
        float dist = (cam.transform.position.x * parallaxEffectX);

        // Update position
        transform.position = new Vector3(startpos + dist, cam.transform.position.y, transform.position.z);

        // Infinite scrolling logic for X-axis
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
}