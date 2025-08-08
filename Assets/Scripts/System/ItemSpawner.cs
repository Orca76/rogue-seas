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

    // --------- 公開API：島生成完了後にこれを呼ぶ ---------
    [ContextMenu("Spawn Items Now")]
    public void SpawnAll()
    {
        if (!ValidateRefs()) return;

        if (seed != 0) UnityEngine.Random.InitState(seed);

        // IslandTile のタイル群をクイック判定用にキャッシュ
        BuildTileTypeCache();

        var bounds = terrainMap.cellBounds;

        // 生成まとめRoot（任意）
        Transform parent = spawnParent ? spawnParent : transform;

        int placed = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (!terrainMap.HasTile(cell)) continue;

                var t = terrainMap.GetTile(cell);
                int tType = ClassifyTile(t);
                if (tType == -1) continue; // 対象外（海など）

                var set = lootByIslandCode[Mathf.Clamp(islandCode, 0, lootByIslandCode.Length - 1)];
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

    // --------- 補助 ---------

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

        // desert
        if (island.desertTile) tileTypeQuick[island.desertTile] = 0;

        // forest
        if (island.forestTile) tileTypeQuick[island.forestTile] = 1;

        // grassland (複数)
        if (island.grasslandTile != null)
        {
            foreach (var g in island.grasslandTile)
                if (g) tileTypeQuick[g] = 2;
        }

        // mountain
        if (island.mountainTile) tileTypeQuick[island.mountainTile] = 3;
    }

    int ClassifyTile(TileBase t)
    {
        if (t == null) return -1;
        if (tileTypeQuick.TryGetValue(t, out int type)) return type;

        // ここに来るのは海など。対象外として -1
        return -1;
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
}
