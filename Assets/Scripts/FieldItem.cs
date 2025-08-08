using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItem : MonoBehaviour
{
    [Header("�A�C�e����{���")]
    public string itemName;
    public Sprite icon;
    public bool isStackable = true;

    public Vector2 alchemyVector;//�B���p
    private void Start()
    {
        // SpriteRenderer���玩���擾
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            icon = sr.sprite;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // �z�b�g�o�[�擾
        HotbarUI hotbar = FindObjectOfType<HotbarUI>();
        if (hotbar != null)
        {
            // �O����`���ꂽ ItemStack �� new ���ēn��
            ItemStack newItem = new ItemStack(itemName, icon, 1, isStackable,alchemyVector);
            bool added = hotbar.TryAddItem(newItem);

            //�Q�b�gUI�̏����擾
            GameObject GetUI = GameObject.Find("ItemGetUI");
            GetUI.GetComponent<ItemGetUIManager>().CreateUI(icon, itemName);

          
            
            if (added)
            {
                Destroy(gameObject); // �E��ꂽ�̂ŏ���
            }
            else
            {
                Debug.Log("�z�b�g�o�[���t");
            }
        }
    }
}