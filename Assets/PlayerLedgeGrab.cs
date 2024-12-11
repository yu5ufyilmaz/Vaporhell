using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLedgeGrab : MonoBehaviour
{

    private bool redBox, greenBox;
    [FormerlySerializedAs("redxOffset")] public float redXOffset;
    [FormerlySerializedAs("redyOffset")] public float redYOffset;
    public float redXSize,redYSize,greenXOffset,greenYOffset,greenXSize,greenYSize;
    private Rigidbody2D rb;

    public bool isGrabbed;
    private float startingGrav;
    public LayerMask groundMask;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingGrav = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        greenBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (greenXOffset*transform.localScale.x), transform.position.y +greenYOffset),new Vector2(greenXSize,greenYSize),0f, groundMask);
        redBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (redXOffset  *transform.localScale.x), transform.position.y +redYOffset),new Vector2(redXSize,redYSize),0f, groundMask);
        if (greenBox && !redBox && !isGrabbed)
        {
            isGrabbed = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (redXOffset  *transform.localScale.x), transform.position.y +redYOffset),new Vector2(redXSize,redYSize));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (greenXOffset  *transform.localScale.x), transform.position.y + greenYOffset),new Vector2(greenXSize,greenYSize));
    }
}
