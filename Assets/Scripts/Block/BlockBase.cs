using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BlockBase : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2Int tilePos;
   
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Interact()
    {
        Debug.Log($"Block at {tilePos} was interacted with.");
    }
}
