using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSync: MonoBehaviour
{
    public Transform playerTransform;

    private float myLayerZ;

    void Start()
    {
        myLayerZ = transform.position.z;

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // ここだけで非アクティブ制御
        gameObject.SetActive(Mathf.Approximately(playerTransform.position.z, myLayerZ));
    }

    void Update()
    {
        bool isSameLayer = Mathf.Approximately(playerTransform.position.z, myLayerZ);
        if (gameObject.activeSelf != isSameLayer)
        {
            gameObject.SetActive(isSameLayer);
        }
    }
}
