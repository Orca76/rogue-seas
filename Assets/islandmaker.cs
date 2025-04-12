using UnityEngine;
using UnityEngine.Tilemaps;

public class IslandMaker : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [Tooltip("配置先の Tilemap")]
    public Tilemap tilemap;
    [Tooltip("海タイル（effective < thresholdSea）")]
    public TileBase seaTile;
    [Tooltip("砂漠タイル（thresholdSea ≤ effective < thresholdDesert）")]
    public TileBase desertTile;
    [Tooltip("草原タイル（thresholdDesert ≤ effective < thresholdGrassland）")]
    public TileBase grasslandTile;
    [Tooltip("森林タイル（thresholdGrassland ≤ effective < thresholdForest）")]
    public TileBase forestTile;
    [Tooltip("山脈タイル（effective ≥ thresholdForest）")]
    public TileBase mountainTile;

    [Header("Map Settings")]
    [Tooltip("グリッドサイズ（2ⁿ+1形式推奨、例: 129）")]
    public int mapWidth = 129;
    public int mapHeight = 129;

    [Header("Diamond-Square Settings")]
    [Tooltip("初期乱数振れ幅。大きいほど起伏が激しくなる (例: 1.0)")]
    public float roughness = 1.0f;
    [Tooltip("各反復毎に roughness に掛かる減衰率 (例: 0.5)")]
    public float decayRate = 0.5f;

    [Header("Boundary Settings")]
    [Tooltip("外縁として固定するセルの幅（セル単位）。例: 3")]
    public int boundaryThickness = 3;

    [Header("Island Mask Settings")]
    [Tooltip("円形マスクの急峻さ。大きいほど外周が急激に低下 (例: 2.0)")]
    public float maskPow = 2f;
    [Tooltip("マスクに引く定数。値を大きくすると全体の高さが下がる (例: 0.0)")]
    public float maskMargin = 0f;

    [Header("Tile Thresholds (Effective Values)")]
    [Tooltip("effective < この値 → 海タイル (例: -0.4)")]
    public float thresholdSea = -0.4f;
    [Tooltip("thresholdSea ≤ effective < この値 → 砂漠タイル (例: -0.1)")]
    public float thresholdDesert = -0.1f;
    [Tooltip("thresholdDesert ≤ effective < この値 → 草原タイル (例: 0.2)")]
    public float thresholdGrassland = 0.2f;
    [Tooltip("thresholdGrassland ≤ effective < この値 → 森林タイル (例: 0.5)")]
    public float thresholdForest = 0.5f;
    // effective ≥ thresholdForest → mountainTile

    [Header("Seed Settings")]
    [Tooltip("内部にランダムに配置するシード点の数 (3～10)")]
    public int minSeedCount = 3;
    public int maxSeedCount = 10; // 上限は排他的

    [Header("Regeneration Settings")]
    [Tooltip("再生成キー (例: Space)")]
    public KeyCode regenerateKey = KeyCode.Space;

    // 内部で生成する高さマップ（2D配列）
    private float[,] heightMap;
    // locked 配列（更新対象から除外するセル）
    private bool[,] locked;

    void Start()
    {
        GenerateIsland();
    }

    void Update()
    {
        if (Input.GetKeyDown(regenerateKey))
        {
            GenerateIsland();
        }
    }

    /// <summary>
    /// 島全体を生成します。
    /// ① 初期化：外縁の boundaryThickness 分は 0 (海) に固定、内部は 0 で初期化
    /// ② 内部からランダムに seedCount (minSeedCount～maxSeedCount-1) 個のセルに 1 (高い) をセットしロック
    /// ③ ダイヤモンドスクエア法で内部補間（ロックされていないセルのみ更新）
    /// ④ FixBoundary() で外縁を再固定
    /// ⑤ 円形マスクで全体を調整
    /// ⑥ effective height に応じてタイルを選定し Tilemap に配置
    /// </summary>
    void GenerateIsland()
    {
        tilemap.ClearAllTiles();

        int w = RoundToPowerOfTwoPlusOne(mapWidth);
        int h = RoundToPowerOfTwoPlusOne(mapHeight);

        heightMap = new float[h, w];
        locked = new bool[h, w];

        // ① 初期状態：外縁を boundaryThickness 分固定＝0（海）
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                if (i < boundaryThickness || i >= h - boundaryThickness ||
                    j < boundaryThickness || j >= w - boundaryThickness)
                {
                    heightMap[i, j] = 0f;
                    locked[i, j] = true;
                }
                else
                {
                    heightMap[i, j] = 0f;
                    // 内部は未ロック
                    locked[i, j] = false;
                }
            }
        }
        // ② 内部にランダムなシード点を配置
        int seedCount = Random.Range(minSeedCount, maxSeedCount); // maxSeedCount は排他的
        for (int k = 0; k < seedCount; k++)
        {
            int randY = Random.Range(boundaryThickness, h - boundaryThickness);
            int randX = Random.Range(boundaryThickness, w - boundaryThickness);
            heightMap[randY, randX] = 1f;
            locked[randY, randX] = true;
        }

        // ③ ダイヤモンドスクエア法で内部を補間（ロックされていないセルのみ更新）
        DiamondSquare(heightMap, locked, w, h, roughness, decayRate);

        // ④ 外縁再固定（万一更新されていた場合のため）
        FixBoundary(heightMap, w, h, boundaryThickness);

        // ⑤ 円形マスクを適用
        ApplyRadialIslandMask(heightMap, w, h, maskPow);

        // ⑥ タイルマップに反映
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float v = heightMap[y, x];
                TileBase tile = ChooseTile(v);
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        Debug.Log("Island generated (size: " + w + "x" + h + ", seeds: " + seedCount + ").");
    }

    /// <summary>
    /// ダイヤモンドスクエア法による高さマップの補間（ロックされていないセルのみ更新）
    /// </summary>
    void DiamondSquare(float[,] map, bool[,] locked, int width, int height, float rough, float decay)
    {
        int stepSize = width - 1;
        float currentRoughness = rough;

        while (stepSize > 1)
        {
            int halfStep = stepSize / 2;

            // ダイヤモンドステップ
            for (int y = halfStep; y < height; y += stepSize)
            {
                for (int x = halfStep; x < width; x += stepSize)
                {
                    // もしこのセルがロックされていれば更新しない
                    if (locked[y, x])
                        continue;

                    float avg = (map[y - halfStep, x - halfStep] +
                                 map[y - halfStep, x + halfStep] +
                                 map[y + halfStep, x - halfStep] +
                                 map[y + halfStep, x + halfStep]) / 4f;
                    map[y, x] = avg + Random.Range(-currentRoughness, currentRoughness);
                }
            }

            // スクエアステップ
            for (int y = 0; y < height; y += halfStep)
            {
                for (int x = ((y / halfStep) % 2 == 0) ? halfStep : 0; x < width; x += stepSize)
                {
                    // もしロックされているなら更新しない
                    if (locked[y, x])
                        continue;

                    float sum = 0f;
                    int count = 0;
                    if (x - halfStep >= 0)
                    {
                        sum += map[y, x - halfStep];
                        count++;
                    }
                    if (x + halfStep < width)
                    {
                        sum += map[y, x + halfStep];
                        count++;
                    }
                    if (y - halfStep >= 0)
                    {
                        sum += map[y - halfStep, x];
                        count++;
                    }
                    if (y + halfStep < height)
                    {
                        sum += map[y + halfStep, x];
                        count++;
                    }
                    float avg = sum / count;
                    map[y, x] = avg + Random.Range(-currentRoughness, currentRoughness);
                }
            }

            stepSize /= 2;
            currentRoughness *= decay;
        }
    }

    /// <summary>
    /// 外縁の boundaryThickness 分のセルを 0 に固定する
    /// </summary>
    void FixBoundary(float[,] map, int width, int height, int thickness)
    {
        // 上下
        for (int y = 0; y < thickness; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[y, x] = 0f;
                map[height - 1 - y, x] = 0f;
            }
        }
        // 左右
        for (int x = 0; x < thickness; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[y, x] = 0f;
                map[y, width - 1 - x] = 0f;
            }
        }
    }

    /// <summary>
    /// 円形マスクを適用。中心からの正規化距離に応じて、外周ほど値を落とします。
    /// </summary>
    void ApplyRadialIslandMask(float[,] map, int width, int height, float power)
    {
        float cx = (width - 1) / 2f;
        float cy = (height - 1) / 2f;
        float maxDist = Mathf.Sqrt(cx * cx + cy * cy);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float normDist = Mathf.Sqrt(dx * dx + dy * dy) / maxDist;
                float mask = 1f - Mathf.Pow(normDist, power);
                mask -= maskMargin;
                mask = Mathf.Clamp01(mask);
                map[y, x] *= mask;
            }
        }
    }

    /// <summary>
    /// effective height の値に応じて、海／砂漠／草原／森林／山脈のタイルを選定する
    /// </summary>
    TileBase ChooseTile(float v)
    {
        if (v < thresholdSea)
            return seaTile;
        else if (v < thresholdDesert)
            return desertTile;
        else if (v < thresholdGrassland)
            return grasslandTile;
        else if (v < thresholdForest)
            return forestTile;
        else
            return mountainTile;
    }

    /// <summary>
    /// 入力値を 2^n+1 形式に丸めます（ダイヤモンドスクエア法が安定して動作するサイズ）
    /// </summary>
    int RoundToPowerOfTwoPlusOne(int val)
    {
        int powerVal = 1;
        while ((powerVal - 1) < val)
        {
            powerVal *= 2;
        }
        int upper = powerVal + 1;
        int lower = (powerVal / 2) + 1;
        int distU = Mathf.Abs(upper - val);
        int distL = Mathf.Abs(lower - val);
        return (distU < distL) ? upper : lower;
    }
}
