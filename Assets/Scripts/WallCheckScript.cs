using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheckScript : MonoBehaviour
{
    private bool isEnable = true;
    public float wallCheckRadius = 0.2f;  // Radius of the circle for wall detection
    public LayerMask wallLayer;       // The layer that represents walls


    public bool IsTouchingWall()
    {
        return Physics2D.OverlapCircle((Vector2)transform.position, wallCheckRadius, wallLayer);
    }
}
