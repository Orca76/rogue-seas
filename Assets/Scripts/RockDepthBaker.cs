using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 岩タイルに「外縁ほど明るい / 奥ほど暗い」頂点カラーを焼く
/// </summary>
public class RockDepthBaker : MonoBehaviour
{
    [SerializeField] int fadeMax = 6;          // 6 タイル奥で真っ黒
    [SerializeField] TileBase rockTile;        // 岩タイルの参照を Inspector でセット

    static readonly Vector3Int[] DIR4 = {
        new( 1, 0, 0), new(-1, 0, 0),
        new( 0, 1, 0), new( 0,-1, 0)
    };

    public void BakeDepth(Tilemap rockMap)
    {
        Debug.Log("<color=cyan>BakeDepth called</color>");
        // --------- ① 奥行き距離を作る ----------
        BoundsInt b = rockMap.cellBounds;
        int w = b.size.x, h = b.size.y;
        int ox = b.xMin, oy = b.yMin;         // オフセット（負座標対策）

        int[,] dist = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                dist[x, y] = -1;

        Queue<Vector3Int> q = new();

        // 外縁セルを列挙
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var cell = new Vector3Int(x + ox, y + oy, 0);
                if (rockMap.GetTile(cell) != rockTile) continue;

                foreach (var d in DIR4)
                {
                    var n = cell + d;
                    if (rockMap.GetTile(n) == null)
                    {     // 隣が空気＝外縁
                        dist[x, y] = 0;
                        q.Enqueue(cell);
                        break;
                    }
                }
            }
        // ── 外縁セル列挙後に追加 ───────────────
        int rockEdgeCount = q.Count;
        Debug.Log($"<color=yellow>外縁セル列挙: {rockEdgeCount}</color>");
        // BFS で奥まで広げる
        while (q.Count > 0)
        {
            var cell = q.Dequeue();
            int cx = cell.x - ox, cy = cell.y - oy;

            foreach (var d in DIR4)
            {
                var n = cell + d;
                int nx = n.x - ox, ny = n.y - oy;
                if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                if (dist[nx, ny] != -1) continue;
                if (rockMap.GetTile(n) != rockTile) continue;   // 岩だけ

                dist[nx, ny] = dist[cx, cy] + 1;
                q.Enqueue(n);
            }
        }

        // --------- ② 頂点カラーを書き込む ----------
        // ── 書き込みループの最後に追加 ──────────
        int colored = 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int d = dist[x, y];
                if (d == -1) continue;                            // 岩以外は無視

               // float t = Mathf.Clamp01(d / (float)fadeMax);      // 0→1
                float t = Mathf.Clamp01(dist[x, y] / (float)fadeMax);
                Color col = (dist[x, y] == 0) ? Color.red    // 外縁を真っ赤
                           : (dist[x, y] == 1) ? Color.green  // 1 マス奥を緑
                           : (dist[x, y] <= 3) ? Color.blue   // 2–3 マス奥を青
                                                     : Color.black;
              

                var cell = new Vector3Int(x + ox, y + oy, 0);
                rockMap.SetTileFlags(cell, TileFlags.None);
                rockMap.SetColor(cell, col);
                colored++;                     // ←カウント
            }
        Debug.Log($"<color=orange>書き込んだセル: {colored}</color>");
    }
   public void BakeDepthNaive(Tilemap map)
    {
        BoundsInt b = map.cellBounds;
        int ox = b.xMin, oy = b.yMin;
        int w = b.size.x, h = b.size.y;

        Vector3Int[] dir4 = {
        new(1,0,0), new(-1,0,0), new(0,1,0), new(0,-1,0)
    };

        bool IsRock(TileBase t)
        {
            return t == rockTile;   // これだけで十分
        }

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var cell = new Vector3Int(x + ox, y + oy, 0);
                if (!IsRock(map.GetTile(cell))) continue;

                int dist = 0;
                bool foundEdge = false;

                // 最大 fadeMax タイル分だけ外側を探す
                for (dist = 1; dist <= fadeMax; dist++)
                {
                    foreach (var d in dir4)
                    {
                        var n = cell + d * dist;
                        var t = map.GetTile(n);
                        if (t == null || !IsRock(t)) { foundEdge = true; break; }
                    }
                    if (foundEdge) break;   // 外に出たので距離が決まった
                }
                if (!foundEdge) dist = fadeMax;   // 奥深すぎたら真っ黒扱い

                // 距離→色
                float tCol = Mathf.Clamp01(dist / (float)fadeMax);
                Color col = Color.Lerp(Color.white, Color.black, tCol);

                map.SetTileFlags(cell, TileFlags.None);
                map.SetColor(cell, col);
            }
    }
}
