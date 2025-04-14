using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCreator : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap tilemap;
    // 各バイオーム用タイル（ここでは川は seaTile を流用）
    public TileBase seaTile;         // 島外、または川
    public TileBase desertTile;      // 内部：砂漠（海岸寄りなど）
    public TileBase grasslandTile;   // 内部：草原
    public TileBase forestTile;      // 内部：森林
    public TileBase mountainTile;    // 内部：山脈

    [Header("Island Outline Settings")]
    // 以下は既存の島輪郭生成用パラメータ
    public float regionWidth = 100f;
    public float regionHeight = 100f;
    public int gridCols = 5;
    public int gridRows = 5;
    public int iterations = 3;
    public float initialDisplacement = 10f;
    public float displacementReduction = 0.5f;

    [Header("Biome & River Noise Settings")]
    [Tooltip("内側のバイオームを決定する低周波ノイズの周波数")]
    public float biomeFrequency = 0.05f;
    [Tooltip("バイオーム判定用ノイズのしきい値（生ノイズ値：-1～+1）")]
    public float biomeThresholdDesert = -0.3f;
    public float biomeThresholdGrassland = 0.0f;
    public float biomeThresholdForest = 0.4f;
    [Tooltip("高周波の川用ノイズの周波数")]
    public float riverFrequency = 0.2f;
    [Tooltip("川として認識するためのノイズの閾値（低ければ川として出る、例:-0.3）")]
    public float riverThreshold = -0.3f;

    [Header("Regeneration")]
    public KeyCode regenerateKey = KeyCode.Space;

    public int mapHeight;
    public int mapWidth;

    // 生成された島の輪郭（多角形）の頂点リスト
    private List<Vector2> islandBoundary;
    // public int noiseSeed をインスペクタなどで設定
    public int noiseSeed = 123;

    // シードに基づく決定的なオフセットを計算する関数
    Vector2 GetNoiseOffset(int seed)
    {
        // 例えば、種に基づく単純な算出方法：
        // seed の数値を何らかの定数で乗算して、固定範囲の数値に調整する方法
        float offsetX = (seed * 12345 % 1000) / 1000f * 100f; // 0〜100
        float offsetY = (seed * 67890 % 1000) / 1000f * 100f; // 0〜100
        return new Vector2(offsetX, offsetY);
    }
    void Start()
    {
        GenerateAndFillMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(regenerateKey))
        {
            GenerateAndFillMap();
        }
    }

    /// <summary>
    /// 島の輪郭を生成し、その内外と内部のバイオームを決定して Tilemap に落とし込みます。
    /// </summary>
    void GenerateAndFillMap()
    {
        // Step 1: 島の輪郭を生成（既存のグリッド＋Midpoint Displacement方式）
        islandBoundary = GenerateIslandPolygon();

        // Step 2: 各タイルを調べ、輪郭内ならバイオーム、外なら海タイルを配置
        FillTilemapFromPolygon(islandBoundary);
        noiseSeed = Random.Range(0, 10000);
    }

    /// <summary>
    /// 5×5のグリッド（角セル除く）から初期の境界点を生成し、Midpoint Displacementで細分化して島境界の多角形を作ります。
    /// </summary>
    List<Vector2> GenerateIslandPolygon()
    {
        List<Vector2> initialPoints = new List<Vector2>();
        float cellWidth = regionWidth / gridCols;
        float cellHeight = regionHeight / gridRows;

        // 上行：行 = gridRows - 1、列 1～gridCols-2
        int topRow = gridRows - 1;
        for (int col = 1; col < gridCols - 1; col++)
        {
            initialPoints.Add(RandomPointInCell(col, topRow, cellWidth, cellHeight));
        }
        // 右列：列 = gridCols - 1、行 gridRows-2～1
        int rightCol = gridCols - 1;
        for (int row = gridRows - 2; row >= 1; row--)
        {
            initialPoints.Add(RandomPointInCell(rightCol, row, cellWidth, cellHeight));
        }
        // 下行：行 = 0、列 gridCols-2～1
        int bottomRow = 0;
        for (int col = gridCols - 2; col >= 1; col--)
        {
            initialPoints.Add(RandomPointInCell(col, bottomRow, cellWidth, cellHeight));
        }
        // 左列：列 = 0、行 1～gridRows-2
        int leftCol = 0;
        for (int row = 1; row < gridRows - 1; row++)
        {
            initialPoints.Add(RandomPointInCell(leftCol, row, cellWidth, cellHeight));
        }
        // 初期は12点
        List<Vector2> poly = new List<Vector2>(initialPoints);
        // Midpoint Displacement を iterations 回実施
        for (int i = 0; i < iterations; i++)
        {
            float disp = initialDisplacement * Mathf.Pow(displacementReduction, i);
            poly = SubdividePolygon(poly, disp);
        }
        return poly;
    }

    // 指定セル (col, row) 内でランダムな点を返す
    Vector2 RandomPointInCell(int col, int row, float cellW, float cellH)
    {
        float marginX = cellW * 0.1f;
        float marginY = cellH * 0.1f;
        float x = col * cellW + Random.Range(marginX, cellW - marginX);
        float y = row * cellH + Random.Range(marginY, cellH - marginY);
        return new Vector2(x, y);
    }

    // Midpoint Displacement：各エッジの中点を求め、ランダムにずらして新たな点として挿入
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

    /// <summary>
    /// 各タイルの中心位置が島の多角形内か判定し、
    /// 内部の場合はバイオームノイズおよび河川ノイズに基づいて生地を決定、外は海Tile。
    /// </summary>
    void FillTilemapFromPolygon(List<Vector2> poly)
    {
        tilemap.ClearAllTiles();
        Vector2 center = new Vector2(regionWidth / 2f, regionHeight / 2f);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // タイル中心
                Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = IsPointInPolygon(pos, poly);
                if (!inside)
                {
                    // 輪郭外は海
                    tilemap.SetTile(new Vector3Int(x, y, 0), seaTile);
                    continue;
                }

                // 内部の場合、2種類のノイズを使う

                // (A) バイオーム用の低周波ノイズ（大きなバイオームブロックを作る）
                // 生成の初めにランダムオフセットを作成しておく
                // 事前に一度、シードに基づくオフセットを計算しておく（例えば Start() で一度だけ）
                Vector2 biomeOffset = GetNoiseOffset(noiseSeed);

                // 各セルでノイズを取得する場合:
                float biomeNoise = Mathf.PerlinNoise((x + biomeOffset.x) * biomeFrequency,
                                                      (y + biomeOffset.y) * biomeFrequency) * 2f - 1f;

                //// (B) 川用の高周波ノイズ（川なら値が低めになることを狙う）
                //float riverNoise = Mathf.PerlinNoise(x * riverFrequency, y * riverFrequency) * 2f - 1f;

                //// 川が出る条件：河川ノイズが一定値未満ならそのセルは川（海タイル）
                //if (riverNoise < riverThreshold)
                //{
                //    tilemap.SetTile(new Vector3Int(x, y, 0), seaTile);
                //    continue;
                //}

                // バイオームの選定は、biomeNoise の値に応じて
                // 例として、以下のように：
                // biomeNoise < biomeThresholdDesert → DesertTile
                // biomeThresholdDesert <= biomeNoise < biomeThresholdGrassland → GrasslandTile
                // biomeThresholdGrassland <= biomeNoise < biomeThresholdForest → ForestTile
                // biomeNoise >= biomeThresholdForest → MountainTile
                TileBase chosenTile = ChooseBiomeTile(biomeNoise);
                tilemap.SetTile(new Vector3Int(x, y, 0), chosenTile);
            }
        }
    }

    /// <summary>
    /// Ray Casting法による、点が多角形内部にあるかの判定
    /// </summary>
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

    /// <summary>
    /// biomeNoise の値に応じてバイオームタイルを選定します。
    /// 推奨例：
    /// biomeNoise < -0.3 → desertTile
    /// -0.3 <= biomeNoise < 0.0 → grasslandTile
    /// 0.0 <= biomeNoise < 0.4 → forestTile
    /// biomeNoise >= 0.4 → mountainTile
    /// </summary>
    TileBase ChooseBiomeTile(float biomeNoise)
    {
        if (biomeNoise < biomeThresholdDesert)
            return desertTile;
        else if (biomeNoise < biomeThresholdGrassland)
            return grasslandTile;
        else if (biomeNoise < biomeThresholdForest)
            return forestTile;
        else
            return mountainTile;
    }

    /// <summary>
    /// 入力値を 2^n+1 形式に丸めます（アルゴリズムが安定するサイズ）
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
