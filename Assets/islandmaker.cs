using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

public class islandmaker : MonoBehaviour
{
    public Tilemap tilemap;
    public RuleTile seaTile, sandTile, mountainTile, desertTile, forestTile, grassTile;

    public int MAP_SIZE = 128;
    public int SEED = 423;
    public int NUM_ANGLES = 72;
    public float BASE_RADIUS = 40f;
    public float RADIUS_NOISE = 20f;
    public float SMOOTH_RADIUS = 2.0f;
    public int BEACH_WIDTH = 1;

    private string[,] mapData;
    private float[,] heightMap, tempMap, rainMap;
    // 1D ガウシアンフィルタ（C#版）
    float[] GaussianSmooth(float[] data, float sigma)
    {
        int size = data.Length;
        float[] result = new float[size];
        float weightSum = 0;
        float[] kernel = new float[size];

        float sigma2 = sigma * sigma;
        for (int i = 0; i < size; i++)
        {
            float x = i - size / 2;
            kernel[i] = Mathf.Exp(-0.5f * x * x / sigma2);
            weightSum += kernel[i];
        }

        for (int i = 0; i < size; i++)
        {
            float smoothedValue = 0;
            for (int j = 0; j < size; j++)
            {
                int idx = Mathf.Clamp(i + j - size / 2, 0, size - 1);
                smoothedValue += data[idx] * kernel[j];
            }
            result[i] = smoothedValue / weightSum;
        }

        return result;
    }
    void Start()
    {
        GenerateIsland();
        tilemap.RefreshAllTiles(); // Tileの適用
    }

    void GenerateIsland()
    {
        mapData = GenerateIslandShape();
        (heightMap, tempMap, rainMap) = GenerateHeightTempRain(mapData);
        AddBeach(mapData, BEACH_WIDTH);
        AssignMountains(mapData, heightMap);
        AssignBiomes(mapData, heightMap, tempMap, rainMap);
        DisplayMap();
    }

    // ----------------------------
    // 1) 島の形を作る（Python版を再現）
    // ----------------------------
    string[,] GenerateIslandShape()
    {
        string[,] map = new string[MAP_SIZE, MAP_SIZE];
        System.Random rand = new System.Random(SEED);
        int cx = MAP_SIZE / 2, cy = MAP_SIZE / 2;

        float[] radii = new float[NUM_ANGLES];
        for (int i = 0; i < NUM_ANGLES; i++)
        {
            radii[i] = BASE_RADIUS + ((float)rand.NextDouble() * 2 - 1) * RADIUS_NOISE;
        }

        // Python の Gaussian Smoothing 相当
        radii = GaussianSmooth(radii, SMOOTH_RADIUS);

        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                float angle = Mathf.Atan2(y - cy, x - cx);
                int idx = Mathf.RoundToInt((angle + Mathf.PI) / (2 * Mathf.PI) * NUM_ANGLES) % NUM_ANGLES;
                float maxDist = radii[idx];

                float dist = Mathf.Sqrt(Mathf.Pow(x - cx, 2) + Mathf.Pow(y - cy, 2));
                map[x, y] = (dist < maxDist) ? "land" : "sea";
            }
        }
        return map;
    }

    // ----------------------------
    // 2) 標高・気温・雨量を作る（Python版を再現）
    // ----------------------------
    (float[,], float[,], float[,]) GenerateHeightTempRain(string[,] map)
    {
        System.Random rand = new System.Random(SEED + 1);
        float[,] height = new float[MAP_SIZE, MAP_SIZE];
        float[,] temp = new float[MAP_SIZE, MAP_SIZE];
        float[,] rain = new float[MAP_SIZE, MAP_SIZE];

        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                height[x, y] = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                temp[x, y] = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                rain[x, y] = Mathf.PerlinNoise(x * 0.07f, y * 0.07f);

                if (map[x, y] == "sea")
                {
                    height[x, y] = temp[x, y] = rain[x, y] = 0f;
                }
            }
        }
        return (height, temp, rain);
    }

    // ----------------------------
    // 3) 外周を砂浜にする
    // ----------------------------
    void AddBeach(string[,] map, int beachWidth)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        int[,] dist = new int[MAP_SIZE, MAP_SIZE];

        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                if (map[x, y] == "sea")
                {
                    dist[x, y] = 0;
                    queue.Enqueue(new Vector2Int(x, y));
                }
                else
                {
                    dist[x, y] = -1;
                }
            }
        }

        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            foreach (Vector2Int dir in dirs)
            {
                Vector2Int newPos = pos + dir;
                if (newPos.x >= 0 && newPos.x < MAP_SIZE && newPos.y >= 0 && newPos.y < MAP_SIZE && dist[newPos.x, newPos.y] == -1)
                {
                    dist[newPos.x, newPos.y] = dist[pos.x, pos.y] + 1;
                    queue.Enqueue(newPos);

                    if (dist[newPos.x, newPos.y] <= beachWidth)
                    {
                        map[newPos.x, newPos.y] = "sand";
                    }
                }
            }
        }
    }

    // ----------------------------
    // 4) 山をシードベースでまとめる
    // ----------------------------
    void AssignMountains(string[,] map, float[,] height)
    {
        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                if (map[x, y] == "land" && height[x, y] > 0.6f)
                {
                    map[x, y] = "mountain";
                }
            }
        }
    }

    // ----------------------------
    // 5) 残りをバイオームに割り当て
    // ----------------------------
    void AssignBiomes(string[,] map, float[,] height, float[,] temp, float[,] rain)
    {
        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                if (map[x, y] == "sea" || map[x, y] == "sand" || map[x, y] == "mountain") continue;

                map[x, y] = (temp[x, y] > 0.7f && rain[x, y] < 0.3f) ? "desert" : (rain[x, y] > 0.5f) ? "forest" : "grass";
            }
        }
    }

    // ----------------------------
    // 6) マップを Tilemap に描画
    // ----------------------------
    void DisplayMap()
    {
        for (int x = 0; x < MAP_SIZE; x++)
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                RuleTile tile = mapData[x, y] switch
                {
                    "sea" => seaTile,
                    "sand" => sandTile,
                    "mountain" => mountainTile,
                    "desert" => desertTile,
                    "forest" => forestTile,
                    "grass" => grassTile,
                    _ => null
                };

                if (tile != null)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
}
