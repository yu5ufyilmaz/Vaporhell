using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetection : MonoBehaviour
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private PlayerController player;

    
    private bool canDetect;
    private void Update()
    {
        if (canDetect)
        {
            player.ledgeDetected = Physics2D.OverlapCircle(player.transform.position, detectionRadius); 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            canDetect = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            canDetect = false;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
