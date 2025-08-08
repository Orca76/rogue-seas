using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItem : MonoBehaviour
{
    [Header("アイテム基本情報")]
    public string itemName;
    public Sprite icon;
    public bool isStackable = true;

    public Vector2 alchemyVector;//錬成用
    private void Start()
    {
        // SpriteRendererから自動取得
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            icon = sr.sprite;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // ホットバー取得
        HotbarUI hotbar = FindObjectOfType<HotbarUI>();
        if (hotbar != null)
        {
            // 外部定義された ItemStack を new して渡す
            ItemStack newItem = new ItemStack(itemName, icon, 1, isStackable,alchemyVector);
            bool added = hotbar.TryAddItem(newItem);

            //ゲットUIの情報を取得
            GameObject GetUI = GameObject.Find("ItemGetUI");
            GetUI.GetComponent<ItemGetUIManager>().CreateUI(icon, itemName);

          
            
            if (added)
            {
                Destroy(gameObject); // 拾われたので消滅
            }
            else
            {
                Debug.Log("ホットバー満杯");
            }
        }
    }
}