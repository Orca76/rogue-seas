using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemSpawner : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] IslandTile island;        // 生成済みの島（あなたのIslandTile）
    [SerializeField] Tilemap terrainMap;       // = island.tilemapSurface を割り当て推奨
    [SerializeField] Transform spawnParent;    // 生成物をまとめる親（任意）

    [Header("島コード 0-5")]
    [Range(0, 5)] public int islandCode = 0;

    [Header("乱数と配置")]
    [SerializeField] int seed = 0;                 // 再現用。0なら使わない
    [Range(0f, 0.5f)] public float cellJitter = 0.15f; // セル内の位置ゆらぎ（セルサイズ比）

    [Serializable]
    public class LootGroup
    {
        [Tooltip("この地形で出す候補（複数ならランダム）")]
        public GameObject[] prefabs;

        [Range(0f, 1f)]
        [Tooltip("この地形セル1マスあたり生成確率")]
        public float chance = 0.2f;
    }

    [Serializable]
    public class IslandLootSet
    {
        [Header("砂浜・砂漠（desertTile）")]
        public LootGroup desert;
        [Header("森林（forestTile）")]
        public LootGroup forest;
        [Header("草原（grasslandTile 群）")]
        public LootGroup grass;
        [Header("山（mountainTile）")]
        public LootGroup mountain;
    }

    [Header("島コードごとのドロップ定義（0〜5）")]
    public IslandLootSet[] lootByIslandCode = new IslandLootSet[6];

    // 内部キャッシュ
    Dictionary<TileBase, int> tileTypeQuick = new(); // 0=desert,1=forest,2=grass,3=mountain

    [Header("Crystal (海以外のタイルで確率湧き)")]
    [SerializeField] GameObject crystalPrefab;
    [Range(0f, 1f)] public float crystalChance = 0.05f;


    // --------- 公開API：島生成完了後にこれを呼ぶ ---------
    [ContextMenu("Spawn Items Now")]
    public void SpawnAll()
    {

       // Debug.Log("Spawn All Called!");
        if (!ValidateRefs()) return;

        if (seed != 0) UnityEngine.Random.InitState(seed);

        // IslandTile のタイル群をクイック判定用にキャッシュ
        BuildTileTypeCache();
        Debug.Log("Spawn All Called2!");
        var bounds = terrainMap.cellBounds;

        // 生成まとめRoot（任意）
        Transform parent = spawnParent ? spawnParent : transform;
        int biomeIdx = GetCurrentBiomeIndex();                 // ★ここ！
        var set = lootByIslandCode[biomeIdx];                  // ★ここ！
        int placed = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);

                if (!terrainMap.HasTile(cell))
                {
                    Debug.Log($"No Tile at {cell}");
                    continue;
                }

                // ===== ここでクリスタル呼び出し =====
                SpawnCrystal(cell, parent);


                var t = terrainMap.GetTile(cell);
                //int tType = ClassifyTile(t);
                //if (tType == -1) continue; // 対象外（海など）
                if (t == null) continue;

                // 水ならスキップ（← これだけでOK）
                if (island.IsWater(t)) continue;

                // 分類（失敗したら草=2にフォールバック）

                int tType = ClassifyByCode(cell);
                //   if (tType == -1) tType = 2; // ← 未登録の陸は草として扱う等、好みで

                //  var set = lootByIslandCode[Mathf.Clamp(islandCode, 0, lootByIslandCode.Length - 1)];
                LootGroup group = GetGroupForType(set, tType);
                if (group == null || group.prefabs == null || group.prefabs.Length == 0) continue;

                // 確率判定
                if (UnityEngine.Random.value > group.chance) continue;

                // 置くやつ選ぶ
                var prefab = group.prefabs[UnityEngine.Random.Range(0, group.prefabs.Length)];
                if (!prefab) continue;

                // セル中心 + ちょいランダム
                Vector3 world = terrainMap.GetCellCenterWorld(cell);
                // セルサイズからオフセット計算
                var cellSize = terrainMap.cellSize;
                world.x += UnityEngine.Random.Range(-cellJitter, cellJitter) * cellSize.x;
                world.y += UnityEngine.Random.Range(-cellJitter, cellJitter) * cellSize.y;

                Instantiate(prefab, world, Quaternion.identity, parent);
                placed++;



                

            }


        Debug.Log($"[ItemSpawner] Spawned {placed} items for islandCode={islandCode}");
    }


    //今の島のバイオームを取る
    int GetCurrentBiomeIndex()
    {
        // IslandTile から Player を引っ張る（なければ Tag から）
        var player = island && island.playerObj
            ? island.playerObj.GetComponent<Player>()
            : GameObject.FindWithTag("Player")?.GetComponent<Player>();

        if (player != null)
            return Mathf.Clamp(player.NextDest, 0, lootByIslandCode.Length - 1);

        // フォールバック：Inspectorの islandCode
        return Mathf.Clamp(islandCode, 0, lootByIslandCode.Length - 1);
    }
    // --------- 補助 ---------

    // クリスタル生成（海タイルは除外）
    void SpawnCrystal(Vector3Int cell, Transform parent)
    {
        if (!crystalPrefab) return;
        if (UnityEngine.Random.value > crystalChance) return;

        var t = terrainMap.GetTile(cell);
        if (t == null) return;

        // ←これだけでシンプルに海判定できる
        if (island.IsWater(t)) return;

        Instantiate(crystalPrefab, JitteredWorld(cell), Quaternion.identity, parent);
    }


    // 既存のセル中心＋ジッターを共通化（未定義なら追加）
    Vector3 JitteredWorld(Vector3Int cell)
    {
        Vector3 world = terrainMap.GetCellCenterWorld(cell);
        var cs = terrainMap.cellSize;
        world.x += UnityEngine.Random.Range(-cellJitter, cellJitter) * cs.x;
        world.y += UnityEngine.Random.Range(-cellJitter, cellJitter) * cs.y;
        return world;
    }

    bool ValidateRefs()
    {
        if (!island)
        {
            Debug.LogError("[ItemSpawner] IslandTile未設定");
            return false;
        }
        if (!terrainMap)
        {
            terrainMap = island.tilemapSurface;
            if (!terrainMap)
            {
                Debug.LogError("[ItemSpawner] Tilemap未設定（island.tilemapSurfaceも空）");
                return false;
            }
        }
        if (lootByIslandCode == null || lootByIslandCode.Length == 0)
        {
            Debug.LogError("[ItemSpawner] lootByIslandCodeが空です（島コードごとの設定を用意して）");
            return false;
        }
        return true;
    }

    void BuildTileTypeCache()
    {
        tileTypeQuick.Clear();

        if (!island)
        {
            Debug.LogWarning("[ItemSpawner] island 未設定");
            return;
        }

        // この spawner が想定する島種（0..5）
        int type = Mathf.Clamp(islandCode, 0, 5);

        // 安全ヘルパー：table[type].variants を辞書に登録
        void AddCategory(UnityEngine.Tilemaps.TileBase[] arr, int code)
        {
            if (arr == null) return;
            foreach (var t in arr) if (t) tileTypeQuick[t] = code;
        }
        void AddFromTable(IslandTile.TileSetByIslandType[] table, int code)
        {
            if (table == null || table.Length == 0) return;
            int idx = Mathf.Clamp(type, 0, table.Length - 1);
            var row = table[idx];
            if (row == null) return;
            AddCategory(row.variants, code);
        }

        // 新方式：島種ごとのバリエーションから登録
        AddFromTable(island.sandTiles, 0); // 砂（砂浜/氷原など）
        AddFromTable(island.forestTiles, 1); // 森（タイガなど）
        AddFromTable(island.grassTiles, 2); // 草（雪原など）
        AddFromTable(island.mountainTiles, 3); // 山（霊峰など）

        // 互換：万一、旧フィールドが生きていたらそれも拾う（任意）
#pragma warning disable 0162, 0219
        try
        {
            // 旧：単体タイル/配列が残ってる場合の救済（存在しなければ無視される）
            var desertField = island.GetType().GetField("desertTile");
            var forestField = island.GetType().GetField("forestTile");
            var grasslandField = island.GetType().GetField("grasslandTile");
            var mountainField = island.GetType().GetField("mountainTile");

            if (desertField != null) { var t = (TileBase)desertField.GetValue(island); if (t) tileTypeQuick[t] = 0; }
            if (forestField != null) { var t = (TileBase)forestField.GetValue(island); if (t) tileTypeQuick[t] = 1; }
            if (grasslandField != null) { var a = (TileBase[])grasslandField.GetValue(island); AddCategory(a, 2); }
            if (mountainField != null) { var t = (TileBase)mountainField.GetValue(island); if (t) tileTypeQuick[t] = 3; }
        }
        catch { /* 無視 */ }
#pragma warning restore 0162, 0219
    }

    // ItemSpawner 内に置く（privateでOK）
    int ClassifyTile(TileBase t)
    {
        if (t == null) return -1;
        return tileTypeQuick.TryGetValue(t, out int type) ? type : -1; // 0=砂,1=森,2=草,3=山, -1=対象外
    }


    LootGroup GetGroupForType(IslandLootSet set, int type)
    {
        return type switch
        {
            0 => set.desert,
            1 => set.forest,
            2 => set.grass,
            3 => set.mountain,
            _ => null
        };
    }
    int ClassifyByCode(Vector3Int cell)
    {
        int x = cell.x, y = cell.y;
        if (x < 0 || y < 0 || x >= island.mapWidth || y >= island.mapHeight) return -1;

        int code = island.tileMapData[x, y];
        // 40台=砂, 10台=森, 20台=草, 30台=山(31含む)
        if (code >= 40 && code < 50) return 0; // desert/氷原
        if (code >= 10 && code < 20) return 1; // forest/タイガ
        if (code >= 20 && code < 30) return 2; // grass/雪原
        if (code >= 30 && code < 40) return 3; // mountain/霊峰
        return -1;
    }


}
