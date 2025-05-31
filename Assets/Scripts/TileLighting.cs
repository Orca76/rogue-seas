using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// タイル単位の松明ライティングを管理するコンポーネント
/// ・Init(tilemap, isGroundLayer) で初期化
/// ・AddTorch(cell) / RemoveTorch(cell) を松明プレハブから呼ぶ
/// ・LateUpdate で dirty なセルだけ Color を再設定
/// </summary>
public class TileLighting : MonoBehaviour
{
    // ───────── 設定値 ─────────
    [SerializeField] int torchPower = 8;          // 半径 8 マス
    [SerializeField] Color litColor = Color.white;
    [SerializeField] Color darkColor = Color.black;

    // ───────── 内部状態 ─────────
    Tilemap map;
    int[,] lightMap;             // 光量 0-8
    List<Vector3Int> dirty = new();   // 色を更新するセル

    int w, h;                     // マップ幅・高さ
    int ox, oy;                   // map.cellBounds.min 用オフセット（負座標対策）

    static readonly Vector3Int[] DIR4 = {
        new ( 1, 0, 0), new (-1, 0, 0),
        new ( 0, 1, 0), new ( 0,-1, 0)
    };

    /*──────────────────────────────────────────────
      初期化
      ￣￣￣￣
      isGroundLayer == true なら全セル光量 8 にして
      地上レイヤ用にする
    ──────────────────────────────────────────────*/
    public void Init(Tilemap tilemap, bool isGroundLayer)
    {
        map = tilemap;

        BoundsInt b = map.cellBounds;         // 使用領域
        w = b.size.x;
        h = b.size.y;
        ox = b.xMin;                          // x,y がマイナスの時の補正
        oy = b.yMin;

        lightMap = new int[w, h];

        int initial = isGroundLayer ? 8 : 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                lightMap[x, y] = initial;
    }

    /*──────────────────────────────────────────────
      松明を置く / 壊す
    ──────────────────────────────────────────────*/
    public void AddTorch(Vector3Int cell) { PropagateLight(cell, +torchPower); }
    public void RemoveTorch(Vector3Int cell) { PropagateLight(cell, -torchPower); }

    void PropagateLight(Vector3Int cell, int deltaPower)
    {
        // セル座標を配列インデックスに変換
        Vector3Int c = cell - new Vector3Int(ox, oy, 0);

        Queue<Vector3Int> q = new(); HashSet<Vector3Int> seen = new();
        q.Enqueue(c); seen.Add(c);

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            int dist = Mathf.Abs(p.x - c.x) + Mathf.Abs(p.y - c.y);
            if (dist > torchPower) continue;

            int newL = Mathf.Clamp(lightMap[p.x, p.y] + deltaPower - dist, 0, 8);
            if (newL == lightMap[p.x, p.y]) continue;          // 変化なし

            lightMap[p.x, p.y] = newL;
            dirty.Add(p);

            foreach (var d in DIR4)
            {
                var n = p + d;
                if (n.x < 0 || n.x >= w || n.y < 0 || n.y >= h) continue;
                if (!seen.Add(n)) continue;
                q.Enqueue(n);
            }
        }
    }

    /*──────────────────────────────────────────────
      変更されたセルだけ頂点カラーを書き換え
    ──────────────────────────────────────────────*/
    void LateUpdate()
    {
        if (dirty.Count == 0) return;

        foreach (var p in dirty)
        {
            int l = lightMap[p.x, p.y];          // 0-8
            float t = 1f - l / 8f;               // 0=明 1=暗
            Color col = Color.Lerp(litColor, darkColor, t);

            // 配列→タイル座標に戻す
            var world = new Vector3Int(p.x + ox, p.y + oy, 0);
            map.SetColor(world, col);
        }
        dirty.Clear();
    }
}
