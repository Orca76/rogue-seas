using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeMapGenerator : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [Tooltip("配置先の Tilemap")]
    public Tilemap tilemap;
    [Tooltip("海タイル（境界および低い高さ）")]
    public TileBase seaTile;
    [Tooltip("草原タイル（中間の高さ）")]
    public TileBase grassTile;
    [Tooltip("山脈タイル（高い高さ）")]
    public TileBase mountainTile;

    [Header("Grid Settings")]
    [Tooltip("グリッドサイズは 2ⁿ+1 形式（例: 129 = 2⁷+1）")]
    public int gridSize = 129;  // mapWidth = mapHeight = gridSize

    [Header("Diamond-Square Settings")]
    [Tooltip("初期の乱数振れ幅。大きいほど起伏が激しくなる")]
    public float roughness = 1.0f;
    [Tooltip("各反復ごとに roughness に掛かる減衰率 (例: 0.5)")]
    public float decayRate = 0.5f;

    [Header("Height Thresholds (Effective Values)")]
    [Tooltip("effective heightがこれ未満なら海タイル。例: 0.2")]
    public float thresholdSea = 0.2f;
    [Tooltip("effective heightがこれ以上なら山脈タイル。例: 0.7")]
    public float thresholdMountain = 0.7f;
    // 間は草原タイル

    [Header("Regeneration")]
    [Tooltip("再生成キー")]
    public KeyCode regenerateKey = KeyCode.Space;

    // 内部で生成する高さマップ
    private float[,] heightMap;

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
    /// ① グリッド（2^n+1×2^n+1）の境界セルはすべて 0、中心セルは 1 に初期化する。
    /// ② ダイヤモンドスクエア法で内側のセルを補間して高さマップを完成させる。
    /// ③ 各セルの effective height に応じて、海／草原／山脈のタイルを Tilemap に配置する。
    /// </summary>
    void GenerateIsland()
    {
        tilemap.ClearAllTiles();

        int size = gridSize;  // 正方形のグリッド
        heightMap = new float[size, size];

        // ① 境界を 0（海）に、中心を 1 に設定する  
        // 境界：i=0, i=size-1, j=0, j=size-1
        for (int j = 0; j < size; j++)
        {
            heightMap[0, j] = 0f;
            heightMap[size - 1, j] = 0f;
        }
        for (int i = 0; i < size; i++)
        {
            heightMap[i, 0] = 0f;
            heightMap[i, size - 1] = 0f;
        }
        // 中心位置
        int center = size / 2;
        heightMap[center, center] = 1f;

        // ② ダイヤモンドスクエア法で内部を補間する
        DiamondSquare(heightMap, size, size, roughness, decayRate);

        // ③ タイルマップに反映
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float v = heightMap[y, x];
                TileBase tile = ChooseTile(v);
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        Debug.Log("Island generated (size: " + size + "x" + size + ").");
    }

    /// <summary>
    /// ダイヤモンドスクエア法の実装
    /// 境界は固定（既に 0 にセット済み）とし、内部を補間していきます。
    /// </summary>
    void DiamondSquare(float[,] map, int width, int height, float rough, float decay)
    {
        int stepSize = width - 1; // 初期ステップは (size-1)
        float currentRoughness = rough;

        // 反復：ステップサイズが 1 以下になるまで続く
        while (stepSize > 1)
        {
            int halfStep = stepSize / 2;

            // ダイヤモンドステップ
            for (int y = halfStep; y < height; y += stepSize)
            {
                for (int x = halfStep; x < width; x += stepSize)
                {
                    // 周囲4点の平均
                    float avg = (map[y - halfStep, x - halfStep] +
                                 map[y - halfStep, x + halfStep] +
                                 map[y + halfStep, x - halfStep] +
                                 map[y + halfStep, x + halfStep]) / 4f;
                    // 中心点に平均とランダムな値を加える
                    map[y, x] = avg + Random.Range(-currentRoughness, currentRoughness);
                }
            }

            // スクエアステップ
            for (int y = 0; y < height; y += halfStep)
            {
                for (int x = ((y / halfStep) % 2 == 0) ? halfStep : 0; x < width; x += stepSize)
                {
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
    /// 高さ値（effective）に応じてタイルを選択する
    /// </summary>
    TileBase ChooseTile(float v)
    {
        if (v < thresholdSea)
            return seaTile;
        else if (v >= thresholdMountain)
            return mountainTile;
        else
            return grassTile;
    }
}
