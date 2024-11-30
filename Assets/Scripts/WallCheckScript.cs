using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheckScript : MonoBehaviour
{
    private bool isEnable = true;
    private bool wasEnable = false;
    public float wallCheckRadius = 0.2f;  // Radius of the circle for wall detection
    public LayerMask wallLayer;       // The layer that represents walls


    public void setEnable()
    {
        isEnable = true;
    }

    public bool IsTouchingWall()
    {
        if (isEnable)
        {
            wasEnable = true;
            return Physics2D.OverlapCircle((Vector2)transform.position, wallCheckRadius, wallLayer);
        }
        else if (wasEnable)
        {
            isEnable = false;
            wasEnable = false;
        }
        return false;
    }
}
