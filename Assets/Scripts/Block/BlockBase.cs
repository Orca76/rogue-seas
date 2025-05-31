using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class BlockBase : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2Int tilePos;

    [SerializeField] public IslandTile islandTiles;  // ← Spawner がセット


    [SerializeField] int maxHp = 1;
    int hp;

    void Awake()
    {
        hp = maxHp;
        // 他の初期化 …

        /* ★ 保険コードを追記 */
        if (islandTiles == null)
            islandTiles = FindObjectOfType<IslandTile>();
    }
    void Start()
    {
        
    }
    public virtual void Damage(int amt = 1)
    {
        hp -= amt;
        if (hp <= 0) Break();               // HP が尽きたら破壊
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Interact()
    {
        Debug.Log($"Block at {tilePos} was interacted with.");
        //if (hp <= 0)
        //{
        //    RockDepthManager.Instance.RequestRebake(tilePos); // ← NEW
        //    Destroy(gameObject);
        //}
    }
    public virtual void Break()
    {
        // ① Tilemap のセルを空にする
        Vector3Int cell = new(tilePos.x, tilePos.y, 0);
        islandTiles.tilemapSurface.SetTile(cell, null);
        islandTiles.tileMapData[tilePos.x, tilePos.y] = 0;

        // ② 深度マネージャに再計算要求
        RockDepthManager.Instance.RequestRebake(tilePos);

        // ③ プレハブ自身を削除
        Destroy(gameObject);
        //// ① Tilemap のセルを床タイルに差し替え
        //TileBase floor = islandTiles.mountainTile;   // ← インスペクタでセット
        //islandTiles.tilemapSurface.SetTile(cell, floor);
        //islandTiles.tileMapData[tilePos.x, tilePos.y] = 37; // データも更新
    }
  
}
