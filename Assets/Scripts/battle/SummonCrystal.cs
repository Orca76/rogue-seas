using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonCrystal : MonoBehaviour
{
    [Header("関連UI")]
    public GameObject alchemyUI;           // 開く対象の錬成UI（非アクティブ状態から切り替え）
    public KeyCode interactKey = KeyCode.E; // インタラクト用キー（例：Eキー）

    private bool playerInRange = false;     // プレイヤーが範囲内にいるかどうか

    private void Update()
    {
        // 範囲内にプレイヤーがいて、かつインタラクトキーが押されたらUIを開く
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            OpenAlchemyUI();
        }
    }

    // UIを開く処理
    private void OpenAlchemyUI()
    {
        alchemyUI.SetActive(true);
        Debug.Log("Alchemy UI opened");
    }

    // プレイヤーがトリガー範囲に入ったとき
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // プレイヤーが範囲から出たとき
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
