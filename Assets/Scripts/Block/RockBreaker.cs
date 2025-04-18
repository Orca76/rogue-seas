using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RockBreaker : MonoBehaviour
{
   

    void Update()
    {
        if (Input.GetMouseButtonDown(0))//ä‚îjâÛèàóù
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                BlockBase block = hit.collider.GetComponent<BlockBase>();
                if (block != null)
                {
                    block.Interact();
                }
            }
        }
    }
}
