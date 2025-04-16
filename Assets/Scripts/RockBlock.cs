using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBlock : BlockBase
{
    public Vector3Int mapPosition;           // 岩の座標
    public IslandTile islandTiles;          // データ保持側の参照（tileMapData持ち）

    public override void Interact()
    {
        // 1. 論理データを更新
        islandTiles.tileMapData[tilePos.x, tilePos.y] = 30;//30は無なので　31が岩のコード
                                                                   // 3. デバッグ出力
        Debug.Log($"岩を破壊！位置: {tilePos.x}, {tilePos.y}");
        // 2. 見た目の削除
        Destroy(gameObject);

       
    }
}
