using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;
using System.Linq;
//using Unity.Mathematics;
using UnityEngine.WSA;
using JetBrains.Annotations;

public class IslandTile : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemapSurface;
    public Tilemap tilemapUnderground;

    [Header("Surface Tiles")]
    public TileBase[] seaTile;
    public TileBase[] shallowWaterTiles;
    public TileBase[] deepWaterTiles;
    //public TileBase desertTile;
    //public TileBase[] grasslandTile;
    //public TileBase forestTile;
    //public TileBase mountainTile;

    //[Header("Underground Tiles")]
    //public TileBase rockTile;
    //public TileBase caveTile; // 通常はnullにする（空洞）
    //public TileBase deepstoneTile;//深層岩
    //public TileBase bedrockTile; // ←これ新規（壊れない岩盤）

    [Header("Island Settings")]
    public float regionWidth = 100f;
    public float regionHeight = 100f;
    public int gridCols = 5;
    public int gridRows = 5;
    public int iterations = 3;
    public float initialDisplacement = 10f;
    public float displacementReduction = 0.5f;

    [Header("Noise Settings")]
    public float biomeFrequency = 0.05f;
    public float biomeThresholdDesert = -0.3f;
    public float biomeThresholdGrassland = 0.0f;
    public float biomeThresholdForest = 0.4f;

    public float biomeThresholdCave;

    [Header("Map Size")]
    public int mapWidth = 100;
    public int mapHeight = 100;

    [Header("Controls")]
    public KeyCode regenerateKey = KeyCode.Space;
    public KeyCode toggleLayerKey = KeyCode.U;

    private List<Vector2> islandBoundary;
    public int noiseSeed = 123;
    private bool showingUnderground = false;


    public int[,] tileMapData;
    public int[,] tileMapDataUnderground;

    public Transform playerTransform; // プレイヤーを参照するために追加
    public float undergroundZ = 1f;   // 地下のZ座標
    public float surfaceZ = 0f;       // 地上のZ座標

    public GameObject islandRoot;//島

    // IslandTile クラスのフィールド群の中に追加  
    static readonly Vector2Int[] DIR8 = {
    new( 1, 0), new(-1, 0), new(0,  1), new(0, -1),
    new( 1, 1), new(-1, 1), new(1, -1), new(-1,-1)
};
    // 距離 0〜4 (幅 5) 用の確率テーブル
    readonly float[] beachChance = { 1f, 0.5f, 0.3f, 0.15f, 0.03f };
    private void Awake()
    {
        tileMapData = new int[mapWidth, mapHeight];
        tileMapDataUnderground = new int[mapWidth, mapHeight];
    }

    // IslandTile フィールドに追記
    float mountainFreq;       // 岩マスク用スケール
    Vector2 mountainOffset;   // 岩マスク用オフセット

    // ------------------ 0. パラメータ ---------------------
    [SerializeField] int shallowWidth = 6;   // 浅瀬の厚み
    [SerializeField] int seaWidth = 16;  // 浅瀬+通常海 の合計厚み
    bool[,] isShore;
    [SerializeField] RockDepthBaker depthBaker;
    [SerializeField] TileLighting lighting;

    public GameObject itemSpawnerObj;//アイテム生成


    public GameObject playerObj;
    Player playerData;

    //こっから新規のタイルセットシステム

    public enum IslandType { Tropical = 0, Frigid = 1, Foggy = 2, Tempest = 3, Organic = 4, Mystic = 5 } // 0..5

    [System.Serializable]
    public class TileSetByIslandType
    {
        // islandType ごとのバリエーション配列（例：6個）
        public TileBase[] variants;
    }

    // 島種ごと × バリエーション6
    [Header("Sand / Forest / Grass / Mountain")]
    public TileSetByIslandType[] sandTiles;     // 長さ = 島種数（6を推奨）
    public TileSetByIslandType[] forestTiles;   // 各要素に variants[0..5]
    public TileSetByIslandType[] grassTiles;
    public TileSetByIslandType[] mountainTiles;

    // 例：共通タイル（必要なら）
    public TileBase[] seaTiles;       // 6
    public TileBase[] shallowTiles;   // 6
    public TileBase[] DeepWaterTiles; // 6




    // ヘルパー：このタイルが「その島種のカテゴリ配列」に含まれるか？
    bool IsInCategory(TileBase tile, TileSetByIslandType[] table, int islandType)
    {
        if (table == null || table.Length == 0) return false;
        islandType = Mathf.Clamp(islandType, 0, table.Length - 1);
        var arr = table[islandType].variants;
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == tile) return true;
        return false;
    }
    void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
        playerData=playerObj.GetComponent<Player>();

        CreateIsland();
        itemSpawnerObj.GetComponent<ItemSpawner>().SpawnAll();//ここでアイテム生成


    }
    public void CreateIsland()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tileMapData[x, y] = 0;
                tileMapDataUnderground[x, y] = 0;
            }
        }

        if (islandRoot != null) // 
            Destroy(islandRoot);
        islandRoot = new GameObject("IslandRoot");
        noiseSeed = Random.Range(0, 10000);
        // CreateIsland() の noiseSeed を決めた直後あたりで
        mountainFreq = Random.Range(1.3f, 2.2f);          // 島ごとにランダム
        mountainOffset = GetNoiseOffset(noiseSeed + 98765); // 別シード

        GenerateAndFillMaps();
    }


    void Update()
    {



        if (Input.GetKeyDown(regenerateKey))
        {
            CreateIsland();
        }
        //if (Input.GetKeyDown(toggleLayerKey))
        //{
        //    ToggleLayer();
        //}

        // プレイヤーのZ座標に応じてレイヤーを切り替える
        bool playerInUnderground = Mathf.Approximately(playerTransform.position.z, undergroundZ);
        tilemapSurface.gameObject.SetActive(!playerInUnderground);
        tilemapUnderground.gameObject.SetActive(playerInUnderground);
    }

    void GenerateAndFillMaps()
    {
        islandBoundary = GenerateIslandPolygon();
        FillSurfaceMap();
      //  FillUndergroundMap();

        // ここで呼び出し
        // depthBaker.BakeDepthNaive(tilemapSurface);   // 地表の岩だけ暗淡
        // lighting.Init(tilemapSurface,true);          // 光量マップ初期化


        RockDepthManager.Instance.RebuildAndBake();
    }

    void ToggleLayer()
    {
        showingUnderground = !showingUnderground;
        tilemapSurface.gameObject.SetActive(!showingUnderground);
        tilemapUnderground.gameObject.SetActive(showingUnderground);
    }

    Vector2 GetNoiseOffset(int seed)
    {
        float offsetX = (seed * 12345 % 1000) / 1000f * 100f;
        float offsetY = (seed * 67890 % 1000) / 1000f * 100f;
        return new Vector2(offsetX, offsetY);
    }

    void FillSurfaceMap()
    {
        tilemapSurface.ClearAllTiles();


        //-p--------------------------

        // ---------- 砂浜生成 (一度だけ) ----------
        isShore = new bool[mapWidth, mapHeight];
        int[,] distMap = new int[mapWidth, mapHeight];
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        // 外周を初期化
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2 p = new Vector2(x + .5f, y + .5f);
                if (!IsPointInPolygon(p, islandBoundary)) continue;
                if (!IsLandTouchingSea(x, y, islandBoundary)) continue;
                MakeSand(x, y, 0, isShore, distMap, q);
            }

        // 幅 5 (距離 0〜4) まで拡張
        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();
            int d = distMap[cur.x, cur.y];
            if (d >= 4) continue;                     // 5 タイル目で打ち切り

            foreach (var dir in DIR8)
            {
                Vector2Int nxt = cur + dir;
                if (nxt.x < 0 || nxt.x >= mapWidth ||
                    nxt.y < 0 || nxt.y >= mapHeight) continue;
                if (isShore[nxt.x, nxt.y]) continue;

                Vector2 np = new Vector2(nxt.x + .5f, nxt.y + .5f);
                if (!IsPointInPolygon(np, islandBoundary)) continue;

                float p = beachChance[d + 1];         // 次の距離 = d+1
                if (Random.value < p)
                    MakeSand(nxt.x, nxt.y, d + 1, isShore, distMap, q);
            }
        }
        // ---------- ここまで砂浜 ----------

        //-----------------------
        Vector2 biomeOffset = GetNoiseOffset(noiseSeed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                if (isShore[x, y]) continue;
                Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                if (!IsPointInPolygon(pos, islandBoundary))
                {
                    tilemapSurface.SetTile(new Vector3Int(x, y, 0), seaTile[Random.Range(0, seaTile.Length)]);



                    continue;
                }


                //  float biomeNoise = Mathf.PerlinNoise((x + biomeOffset.x) * biomeFrequency, (y + biomeOffset.y) * biomeFrequency) * 2f - 1f;

                float nx = (x + biomeOffset.x) * biomeFrequency;
                float ny = (y + biomeOffset.y) * biomeFrequency;

                // 基本のノイズ
                float baseNoise = Mathf.PerlinNoise(nx, ny); // 0〜1

                // 追加1：低周波ノイズ（大きなバイオームの「塊」感）
                float patchNoise = Mathf.PerlinNoise(nx * 0.3f, ny * 0.3f); // 0〜1

                // 追加2：高周波ノイズ（境界のざらつき）
                float detailNoise = Mathf.PerlinNoise(nx * 2.5f, ny * 2.5f); // 0〜1

                // 合成（重みは自由に調整可能）
                float combined = baseNoise * 0.5f + patchNoise * 0.35f + detailNoise * 0.15f;

                // 出力を -1〜1 にマッピング
                float biomeNoise = combined * 2f - 1f;

                // ② 岩マスクノイズを別に評価　――――――――――――――――――――――
                float ridgeNoise = Mathf.PerlinNoise(
                      (x + mountainOffset.x) * mountainFreq,
                      (y + mountainOffset.y) * mountainFreq);            // 0〜1 で“筋”が出る

                // ③ 「既に高い」+「ridge が濃い」セルだけ岩に昇格
                //    山にしたい濃さは好きに調整: combined>0.65, ridge>0.78 など
                if (combined > 0.65f && ridgeNoise > 0.78f)
                    biomeNoise = 0.8f;    // → 0.4 以上なので ChooseBiomeTile は mountainTile を返す




                //kokokara
              

                TileBase tile = ChooseBiomeTile(biomeNoise); // デフォは海

                int islandType = Mathf.Clamp(playerData.NextDest, 0, 5);

                if (IsInCategory(tile, sandTiles, islandType)) tileMapData[x, y] = 40; // 砂
                else if (IsInCategory(tile, forestTiles, islandType)) tileMapData[x, y] = 10; // 森
                else if (IsInCategory(tile, grassTiles, islandType)) tileMapData[x, y] = 20; // 草
                else if (IsInCategory(tile, mountainTiles, islandType)) tileMapData[x, y] = 31; // 山（岩確定）

                //if (tile == desertTile)
                //{
                //    int[] options = { 40 }; //無
                //    tileMapData[x, y] = options[Random.Range(0, options.Length)];
                //}

                //if (tile == forestTile)
                //{
                //    int[] options = { 10 }; // 無　木　花　草
                //    tileMapData[x, y] = options[Random.Range(0, options.Length)];
                //}
                //if (grasslandTile.Contains(tile))
                //{
                //    int[] options = { 20 }; // 無　木　花　草
                //    tileMapData[x, y] = options[Random.Range(0, options.Length)];
                //}
                //if (tile == mountainTile)
                //{
                //    int[] options = { 30, 31 }; //無 岩　鉱石　鉱石　鉱石
                //    tileMapData[x, y] = 31;//まずは確定で岩を生成
                //}

                // === 水域：ここに浅瀬判定を差し込む ===============================

                // ================================================================

                tilemapSurface.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        PaintWaterBands();
    }

    //void FillUndergroundMap()
    //{
    //    tilemapUnderground.ClearAllTiles();
    //    Vector2 biomeOffset = GetNoiseOffset(noiseSeed + 999);

    //    for (int y = 0; y < mapHeight; y++)
    //    {
    //        for (int x = 0; x < mapWidth; x++)
    //        {
    //            Vector3Int pos3 = new Vector3Int(x, y, 0);
    //            Vector2 pos2 = new Vector2(x + 0.5f, y + 0.5f);

    //            // 地上が海のロジック → 地下は bedrock
    //            if (!IsPointInPolygon(pos2, islandBoundary))
    //            {
    //                tilemapUnderground.SetTile(pos3, bedrockTile);
    //                int[] options = { 60 }; //岩盤
    //                tileMapDataUnderground[x, y] = options[Random.Range(0, options.Length)];
    //                continue;
    //            }

    //            // 地下ノイズによる空洞 or 岩タイルの配置
    //            float biomeNoise = Mathf.PerlinNoise((x + biomeOffset.x) * biomeFrequency,
    //                                                  (y + biomeOffset.y) * biomeFrequency) * 2f - 1f;

    //            if (biomeNoise < biomeThresholdCave)
    //            {
    //                tilemapUnderground.SetTile(pos3, caveTile); // 空洞
    //                int[] options = { 40 }; //無(地下洞窟)
    //                tileMapDataUnderground[x, y] = options[Random.Range(0, options.Length)];
    //            }
    //            else
    //            {
    //                tilemapUnderground.SetTile(pos3, deepstoneTile); // 岩
    //                int[] options = { 50 }; //深層岩
    //                tileMapDataUnderground[x, y] = options[Random.Range(0, options.Length)];
    //            }


    //        }
    //    }
    //}

    // まずヘルパーをクラス内に追加（IslandTile のどこか上の方でOK）
    TileBase PickVariant(TileSetByIslandType[] table, int islandType, int salt = 0)
    {
        if (table == null || table.Length == 0) return null;
        islandType = Mathf.Clamp(islandType, 0, table.Length - 1);

        var arr = table[islandType]?.variants;
        if (arr == null || arr.Length == 0) return null;

        // 再現性が欲しければ deterministic、ランダムで良ければ Random.Range に変えてOK
        unchecked
        {
            int h = noiseSeed * 73856093 ^ islandType * 19349663 ^ salt * 83492791;
            if (h == int.MinValue) h = 0;
            int i = Mathf.Abs(h) % arr.Length;
            return arr[i];
        }
    }

    // 最終フォールバック（どこかから1枚でも返す）
    TileBase AnyFallback()
    {
        // 好きな優先順で。ここでは草→砂→森→山
        TileBase t;
        t = PickVariant(grassTiles, 0); if (t) return t;
        t = PickVariant(sandTiles, 0); if (t) return t;
        t = PickVariant(forestTiles, 0); if (t) return t;
        t = PickVariant(mountainTiles, 0); if (t) return t;
        return null;
    }



    TileBase ChooseBiomeTile(float val)
    {
        // 島種 0..5（未設定なら 0）
        int islandType = 0;
        if (playerData != null) islandType = Mathf.Clamp(playerData.NextDest, 0, 5);

        // しきい値はそのまま使用：val をカテゴリに振り分ける
        if (val < biomeThresholdDesert)
        {
            // 砂（温暖なら砂、寒冷なら氷原など、インスペクタで入れたもの）
            return PickVariant(sandTiles, islandType, salt: 1) ?? AnyFallback();
        }
        else if (val < biomeThresholdGrassland)
        {
            // 草原（雪原など）
            return PickVariant(grassTiles, islandType, salt: 2) ?? AnyFallback();
        }
        else if (val < biomeThresholdForest)
        {
            // 森（タイガなど）
            return PickVariant(forestTiles, islandType, salt: 3) ?? AnyFallback();
        }
        else
        {
            // 山（霊峰など）
            return PickVariant(mountainTiles, islandType, salt: 4) ?? AnyFallback();
        }
    }

    //TileBase ChooseBiomeTile(float val)
    //{
    //    if (val < biomeThresholdDesert)
    //        return desertTile;
    //    else if (val < biomeThresholdGrassland)
    //        return grasslandTile[Random.Range(0, grasslandTile.Length)];
    //    else if (val < biomeThresholdForest)
    //        return forestTile;
    //    else
    //        return mountainTile;
    //}



    List<Vector2> GenerateIslandPolygon()
    {
        List<Vector2> points = new List<Vector2>();
        float cellW = regionWidth / gridCols;
        float cellH = regionHeight / gridRows;

        for (int col = 1; col < gridCols - 1; col++)
            points.Add(RandomPointInCell(col, gridRows - 1, cellW, cellH));
        for (int row = gridRows - 2; row >= 1; row--)
            points.Add(RandomPointInCell(gridCols - 1, row, cellW, cellH));
        for (int col = gridCols - 2; col >= 1; col--)
            points.Add(RandomPointInCell(col, 0, cellW, cellH));
        for (int row = 1; row < gridRows - 1; row++)
            points.Add(RandomPointInCell(0, row, cellW, cellH));

        List<Vector2> poly = new List<Vector2>(points);
        for (int i = 0; i < iterations; i++)
        {
            float disp = initialDisplacement * Mathf.Pow(displacementReduction, i);
            poly = SubdividePolygon(poly, disp);
        }
        return poly;
    }

    Vector2 RandomPointInCell(int col, int row, float cellW, float cellH)
    {
        float marginX = cellW * 0.1f;
        float marginY = cellH * 0.1f;
        float x = col * cellW + Random.Range(marginX, cellW - marginX);
        float y = row * cellH + Random.Range(marginY, cellH - marginY);
        return new Vector2(x, y);
    }

    List<Vector2> SubdividePolygon(List<Vector2> poly, float maxDisp)
    {
        List<Vector2> newPoly = new List<Vector2>();
        int count = poly.Count;
        for (int i = 0; i < count; i++)
        {
            Vector2 p0 = poly[i];
            Vector2 p1 = poly[(i + 1) % count];
            newPoly.Add(p0);
            Vector2 mid = (p0 + p1) / 2f;
            mid.x += Random.Range(-maxDisp, maxDisp);
            mid.y += Random.Range(-maxDisp, maxDisp);
            newPoly.Add(mid);
        }
        return newPoly;
    }

    bool IsPointInPolygon(Vector2 point, List<Vector2> poly)
    {
        bool inside = false;
        int count = poly.Count;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 pi = poly[i];
            Vector2 pj = poly[j];
            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
    float GetDistanceToIslandEdge(Vector2 point)
    {
        float minDist = float.MaxValue;
        foreach (Vector2 edgePoint in islandBoundary)
        {
            float dist = Vector2.Distance(point, edgePoint);
            if (dist < minDist)
                minDist = dist;
        }
        return minDist;
    }
    // 既存メソッドの下あたりに貼り付け
    void MakeSand(int x, int y, int dist,
                  bool[,] isShore, int[,] distMap,
                  Queue<Vector2Int> q)
    {
        isShore[x, y] = true;
        distMap[x, y] = dist;

        // 島種（0..5）は playerData.NextDist から
        int islandType = 0;
        if (playerData != null) islandType = Mathf.Clamp(playerData.NextDest, 0, 5);

        // 砂カテゴリからビーチ用タイルを1枚選ぶ（未設定ならフォールバック）
        TileBase beach = PickVariant(sandTiles, islandType, salt: 100 + dist) ?? AnyFallback();
        tilemapSurface.SetTile(new Vector3Int(x, y, 0), beach);

        tileMapData[x, y] = 40; // 既存ロジック踏襲（ビーチ=40）
        q.Enqueue(new Vector2Int(x, y));
    }

    bool IsLandTouchingSea(int x, int y, List<Vector2> boundary)
    {
        foreach (var d in DIR8)
        {
            int nx = x + d.x;
            int ny = y + d.y;
            Vector2 np = new Vector2(nx + .5f, ny + .5f);

            if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight) return true; // 外＝海
            if (!IsPointInPolygon(np, boundary)) return true; // 隣が海
        }
        return false;
    }
    // 補助: 水判定（あなたの seaTile 配列に合わせて）
    public bool IsWater(TileBase t)
    {
        if (t == null) return false;

        // 深海チェック
        if (deepWaterTiles != null)
            foreach (var deep in deepWaterTiles)
                if (t == deep) return true;

        // 浅瀬チェック
        if (shallowWaterTiles != null)
            foreach (var s in shallowWaterTiles)
                if (t == s) return true;

        // 海タイルチェック
        if (seaTile != null)
            foreach (var sea in seaTile)
                if (t == sea) return true;

        return false;
    }


    void PaintWaterBands()
    {
        int[,] dist = new int[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                dist[x, y] = -1;

        Queue<Vector2Int> q = new Queue<Vector2Int>();

        /* 1. 砂浜セルをキューに投入 */
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                if (isShore[x, y])          // ← さっき作った配列をそのまま使える
                {
                    q.Enqueue(new Vector2Int(x, y));
                    dist[x, y] = 0;
                }

        /* 2. 4 近傍で水タイル側へ BFS 拡張 */
        Vector2Int[] dirs = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
        while (q.Count > 0)
        {
            var p = q.Dequeue();
            foreach (var d in dirs)
            {
                int nx = p.x + d.x, ny = p.y + d.y;
                if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight) continue;
                if (dist[nx, ny] != -1) continue;              // 既訪

                // 水タイルだけを伸ばす
                if (!IsWater(tilemapSurface.GetTile(new Vector3Int(nx, ny, 0)))) continue;

                dist[nx, ny] = dist[p.x, p.y] + 1;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }

        /* 3. 距離でタイルを再ペイント */
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
            {
                int d = dist[x, y];
                if (d == -1) continue;                     // 陸 or 砂浜

                //TileBase newTile =
                //      (d <= shallowWidth) ? shallowWaterTile
                //    : (d <= seaWidth) ? seaTile[Random.Range(0, seaTile.Length)]
                //                                : deepWaterTile;
                TileBase newTile;

                if (d <= shallowWidth) // 浅瀬
                {
                    if (shallowWaterTiles != null && shallowWaterTiles.Length > 0)
                        newTile = shallowWaterTiles[Random.Range(0, shallowWaterTiles.Length)];
                    else
                        newTile = null; // 後でフォールバック
                }
                else if (d <= seaWidth) // 通常海
                {
                    if (seaTile != null && seaTile.Length > 0)
                        newTile = seaTile[Random.Range(0, seaTile.Length)];
                    else
                        newTile = null;
                }
                else // 深海
                {
                    if (deepWaterTiles != null && deepWaterTiles.Length > 0)
                        newTile = deepWaterTiles[Random.Range(0, deepWaterTiles.Length)];
                    else
                        newTile = null;
                }

                tilemapSurface.SetTile(new Vector3Int(x, y, 0), newTile);
            }
    }

}

