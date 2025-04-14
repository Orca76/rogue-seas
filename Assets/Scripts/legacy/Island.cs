using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Island : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap tilemap;
    // 内部は陸タイル、外部は海タイル（両方とも 1 種類のみ）
    public TileBase islandTile;
    public TileBase waterTile;

    [Header("Grid & Region Settings")]
    [Tooltip("生成領域の幅（ユニット）。タイルサイズを1と仮定します")]
    public float regionWidth = 100f;
    [Tooltip("生成領域の高さ（ユニット）")]
    public float regionHeight = 100f;
    [Tooltip("グリッドの列数（ここでは5固定）")]
    public int gridCols = 5;
    [Tooltip("グリッドの行数（ここでは5固定）")]
    public int gridRows = 5;

    [Header("Midpoint Displacement Settings")]
    [Tooltip("中点ずらしの反復回数")]
    public int iterations = 3;
    [Tooltip("初回中点ずらしでの最大ずらし量（ユニット）")]
    public float initialDisplacement = 10f;
    [Tooltip("各反復ごとにずらし量を減らす係数 (例: 0.5なら半分ずつ減少)")]
    public float displacementReduction = 0.5f;

    [Header("Tilemap Fill Settings")]
    [Tooltip("タイルマップの横幅（セル数）。ここでは regionWidth と同じ想定")]
    public int mapWidth = 100;
    [Tooltip("タイルマップの縦幅（セル数）")]
    public int mapHeight = 100;

    [Header("Regeneration")]
    [Tooltip("再生成するキー（デフォルトは Space）")]
    public KeyCode regenerateKey = KeyCode.Space;

    // 内部で生成された島の境界（多角形）を格納するリスト
    private List<Vector2> islandBoundary;

    void Start()
    {
        GenerateAndFillIsland();
    }

    void Update()
    {
        if (Input.GetKeyDown(regenerateKey))
        {
            GenerateAndFillIsland();
        }
    }

    /// <summary>
    /// 島の境界（多角形）を生成し、その内部／外部によってタイルマップに陸／海タイルを配置する。
    /// </summary>
    void GenerateAndFillIsland()
    {
        // Step 1: 島の境界（多角形）を生成
        islandBoundary = GenerateIslandPolygon();

        // Step 2: その多角形の内外を判定し、タイルマップに落とし込む
        FillTilemapFromPolygon(islandBoundary);
    }

    /// <summary>
    /// 5×5のグリッド（角セル除く）から初期の境界点を作り、Midpoint Displacementで細分化して島境界を生成する
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
        // 右列：列 = gridCols - 1、行 gridRows-2～1 (下方向)
        int rightCol = gridCols - 1;
        for (int row = gridRows - 2; row >= 1; row--)
        {
            initialPoints.Add(RandomPointInCell(rightCol, row, cellWidth, cellHeight));
        }
        // 下行：行 = 0、列 gridCols-2～1 (逆方向)
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
        // 初期の境界点は 12 点になる

        List<Vector2> poly = new List<Vector2>(initialPoints);
        // Midpoint Displacement を iterations 回実施
        for (int i = 0; i < iterations; i++)
        {
            float disp = initialDisplacement * Mathf.Pow(displacementReduction, i);
            poly = SubdividePolygon(poly, disp);
        }
        return poly;
    }

    // 指定されたグリッドセル（col, row）の内部で、セルの端を避けたランダムな点を返す
    Vector2 RandomPointInCell(int col, int row, float cellW, float cellH)
    {
        float marginX = cellW * 0.1f;
        float marginY = cellH * 0.1f;
        float x = col * cellW + Random.Range(marginX, cellW - marginX);
        float y = row * cellH + Random.Range(marginY, cellH - marginY);
        return new Vector2(x, y);
    }

    // ポリゴンの各エッジの中点を求め、ランダムにずらして新しい頂点として挿入
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
    /// タイルマップの各セル（タイルの中心）が、多角形（島境界）の内側かどうか判定し、
    /// 内側なら島タイル、外側なら海タイルを配置する。
    /// </summary>
    void FillTilemapFromPolygon(List<Vector2> poly)
    {
        tilemap.ClearAllTiles();

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // 各タイルの中心座標（タイルサイズ 1 ユニットとして）
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = IsPointInPolygon(p, poly);
                TileBase chosenTile = inside ? islandTile : waterTile;
                tilemap.SetTile(new Vector3Int(x, y, 0), chosenTile);
            }
        }
    }

    /// <summary>
    /// Ray Casting 法による点が多角形内部にあるかの判定
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
}
