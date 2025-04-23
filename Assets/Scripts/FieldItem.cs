using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public ItemData data; // ← Inspector で .asset をドラッグしてセット！

    private void OnTriggerEnter2D(Collider2D other)
    {

        Debug.Log(other.name);

        if (!other.CompareTag("Player")) return;

        // ホットバー取得
        HotbarUI hotbar = FindObjectOfType<HotbarUI>();
        if (hotbar != null)
        {
            bool added = hotbar.TryAddItem(data);
            if (added)
            {
                Destroy(gameObject); // 拾われたら消す
            }
            else
            {
                Debug.Log("ホットバー満杯");
            }
        }
    }
}
