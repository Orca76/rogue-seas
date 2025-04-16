using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RockBreaker : MonoBehaviour
{
    public Tilemap rockTilemap; // 岩のタイルマップ
    public TileBase destroyedTile; // 岩を壊した後の見た目（nullでも可）

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 左クリック
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = rockTilemap.WorldToCell(mouseWorldPos);

            if (rockTilemap.HasTile(cellPos))
            {
                // 壊す処理
                rockTilemap.SetTile(cellPos, destroyedTile); // or null
                Debug.Log("岩を破壊！");
            }
        }
    }
}
