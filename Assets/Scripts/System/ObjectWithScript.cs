using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectWithScript<T> where T : Component
{
    public GameObject obj;

    public T Script => obj.GetComponent<T>();

    public Transform transform => obj.transform;
    public string name => obj.name;
    // ‘¼‚É‚à‚æ‚­Žg‚¤‚â‚Â“ü‚ê‚ÄOK
}