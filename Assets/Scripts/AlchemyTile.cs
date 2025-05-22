using UnityEngine;
using UnityEngine.Tilemaps;

public class AlchemyTile : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tilemapAlchemy;

    [Header("Tile Types")]
    public TileBase commonTile;     // 青
    public TileBase rareTile;       // 紫
    public TileBase superRareTile;  // 黄

    [Header("Map Size")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Noise Settings")]
    public float scale = 0.05f;
    public int noiseSeed = 1234;


    [Header("Control Keys")]
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode hideKey = KeyCode.T;
    private void Start()
    {
       // GenerateAlchemyMap();
    }
    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            noiseSeed = Random.Range(0, 10000);
            tilemapAlchemy.gameObject.SetActive(true);
            GenerateAlchemyMap();
        }

        if (Input.GetKeyDown(hideKey))
        {
            tilemapAlchemy.gameObject.SetActive(false);
        }
    }
    public void GenerateAlchemyMap()
    {
        tilemapAlchemy.ClearAllTiles();

        Vector2 offset = GetNoiseOffset(noiseSeed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float nx = (x + offset.x) * scale;
                float ny = (y + offset.y) * scale;
                float noiseVal = Mathf.PerlinNoise(nx, ny);

                float maxDist = Mathf.Sqrt(Mathf.Pow(mapWidth / 2f, 2) + Mathf.Pow(mapHeight / 2f, 2));
                float dx = x - mapWidth / 2f;
                float dy = y - mapHeight / 2f;
                float distFactor = Mathf.Sqrt(dx * dx + dy * dy) / maxDist;

                TileBase selectedTile = GetTileByNoise(noiseVal, distFactor);
                tilemapAlchemy.SetTile(new Vector3Int(x, y, 0), selectedTile);
            }
        }
    }

    private TileBase GetTileByNoise(float val, float distFactor)
    {
        // 外側ほどレアが出やすくするために閾値を調整（線形補正）
        float commonThreshold = Mathf.Lerp(0.6f, 0.45f, distFactor);
        float rareThreshold = Mathf.Lerp(0.975f, 0.65f, distFactor);

        if (val < commonThreshold)
            return commonTile;
        else if (val < rareThreshold)
            return rareTile;
        else
            return superRareTile;
    }

    private Vector2 GetNoiseOffset(int seed)
    {
        float offsetX = (seed * 12345 % 1000) / 1000f * 100f;
        float offsetY = (seed * 67890 % 1000) / 1000f * 100f;
        return new Vector2(offsetX, offsetY);
    }
}
