using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public ItemData data; // �� Inspector �� .asset ���h���b�O���ăZ�b�g�I

    private void OnTriggerEnter2D(Collider2D other)
    {

        Debug.Log(other.name);

        if (!other.CompareTag("Player")) return;

        // �z�b�g�o�[�擾
        HotbarUI hotbar = FindObjectOfType<HotbarUI>();
        if (hotbar != null)
        {
            bool added = hotbar.TryAddItem(data);
            if (added)
            {
                Destroy(gameObject); // �E��ꂽ�����
            }
            else
            {
                Debug.Log("�z�b�g�o�[���t");
            }
        }
    }
}
