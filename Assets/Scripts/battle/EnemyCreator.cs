using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCreator : MonoBehaviour
{
    public int enemyLimitNum;         // 最大同時出現数（生存数）
    public int enemyCreateLimit;      // 総スポーン上限（例：120）
    public int KilledEnemyNum = 0;    // 撃破数（外部から増える想定）
    public float Radiusmin;           // 最小スポーン距離
    public float RadiusMax;           // 最大スポーン距離
   // public GameObject[] Enemies;      // 通常敵プレハブ
    public float CreateSpan;          // 敵生成間隔

    GameObject player;
    float lastSpawnTime;

    public Image ProgressBar;         // 敵進捗バー（Type=Filled推奨）
    public GameObject Startpoint;     // プログレスバー下端（RectTransform推奨）
    public GameObject Endpoint;       // プログレスバー上端（RectTransform推奨）
    public GameObject WarningIcon;    // 精鋭アイコン用UIプレハブ（Image付き）
    public int EliteNum;              // 精鋭の数（例：5）
    public GameObject[] EliteEnemies; // 精鋭プレハブ（複数ならランダム）

    // ======================== //
    // [ADD] 内部管理フィールド  //
    // ======================== //
    int _totalSpawned = 0;                 // これまでスポーンした総数（1始まり運用）
    List<int> _eliteIndices = new List<int>(); // 1..enemyCreateLimit の中から重複なしで EliteNum 個
    bool _elitesInitialized = false;




    //敵まとめる
    // ネストクラス（内部クラス）
    [System.Serializable]
    public class IslandEnemySet
    {
        public GameObject[] enemies = new GameObject[4];   // 島ごとに4体
    }

    // 島ごとにまとめた敵リスト（5島分）
    public IslandEnemySet[] islandEnemySets = new IslandEnemySet[5];

    // 敵取得用メソッド
    public GameObject GetEnemy(int islandIndex, int enemyIndex)
    {
        return islandEnemySets[islandIndex].enemies[enemyIndex];
    }
    //今いる島コード　敵コード　位置
    public void SpawnEnemy(int islandIndex, int enemyIndex, Vector3 pos)
    {
        GameObject enemyPrefab = GetEnemy(islandIndex, enemyIndex);
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }

    int islandeCode;//どの島？
    void Start()
    {
        player = GameObject.FindWithTag("Player"); // プレイヤーを取得
        lastSpawnTime = Time.time;
        islandeCode = player.GetComponent<Player>().NextDest;//0なら温暖海域、、、
        //精鋭の抽選とアイコン配置
        InitEliteIndices();
        PlaceEliteIcons();
        UpdateProgressBar(); // 初期表示
    }

    void Update()
    {
        //プログレスバー更新（KilledEnemyNum は外部で増える前提）
        UpdateProgressBar();

        // 総スポーンが上限に達したら以降はスポーンしない
        if (_totalSpawned >= enemyCreateLimit) return;

        // 敵が最大数以下かつ、生成間隔を満たしたらスポーン
        if (CountEnemies() < enemyLimitNum && Time.time - lastSpawnTime >= CreateSpan)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
    }

    void SpawnEnemy()
    {
        //次にスポーンする通し番号（1始まり）
        int nextIndex = _totalSpawned + 1;

        //上限超え防止（保険）
        if (nextIndex > enemyCreateLimit) return;

        //精鋭かどうか
        bool isElite = IsEliteIndex(nextIndex);

        Vector2 spawnPos = setCreatePosition();

        GameObject prefab;
        if (isElite && EliteEnemies != null && EliteEnemies.Length > 0)
        {
            prefab = EliteEnemies[islandeCode];
        }
        else
        {
            // その島に登録されてる敵リストの長さを参照
            int enemyIndex = Random.Range(0, islandEnemySets[islandeCode].enemies.Length);
            prefab = GetEnemy(islandeCode, enemyIndex);
        }
        Debug.Log(prefab);
        Instantiate(prefab, spawnPos, Quaternion.identity);

        // 総スポーン数カウントアップ
        _totalSpawned = nextIndex;
    }

    Vector2 setCreatePosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Random.value) * (RadiusMax - Radiusmin) + Radiusmin;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        return (Vector2)player.transform.position + offset;
    }

    int CountEnemies()
    {
        // "Enemy" タグが付いている敵をカウント（事前にプレハブに Enemy タグをつけて）
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    // ================================= //
    // [ADD] ここから：精鋭＆UIまわり   //
    // ================================= //

    // 精鋭の出現番号を重複なしで抽選（1..enemyCreateLimit の中から EliteNum 個）
    void InitEliteIndices()
    {
        if (_elitesInitialized) return;
        _eliteIndices.Clear();

        int max = Mathf.Max(0, enemyCreateLimit);
        int need = Mathf.Clamp(EliteNum, 0, max);

        if (need == 0 || max == 0)
        {
            _elitesInitialized = true;
            return;
        }

        // シンプルなシャッフル方式でユニーク抽選
        List<int> pool = new List<int>(max);
        for (int i = 1; i <= max; i++) pool.Add(i);

        // フィッシャー–イェーツ
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        for (int k = 0; k < need; k++) _eliteIndices.Add(pool[k]);
        _eliteIndices.Sort(); // 並び順を分かりやすく

        _elitesInitialized = true;
    }

    // 指定の通し番号（1始まり）が精鋭か
    bool IsEliteIndex(int index1Based)
    {
        if (!_elitesInitialized) return false;
        // 線形で十分（EliteNum少ない前提）。気になるなら HashSet 化
        for (int i = 0; i < _eliteIndices.Count; i++)
        {
            if (_eliteIndices[i] == index1Based) return true;
        }
        return false;
    }

    // 指定の通し番号が全体の何割か（0..1）
    float ProgressOfIndex(int index1Based)
    {
        if (enemyCreateLimit <= 0) return 0f;
        return Mathf.Clamp01((float)index1Based / (float)enemyCreateLimit);
    }

    // 精鋭アイコンをバー上に全て配置
    void PlaceEliteIcons()
    {
        if (WarningIcon == null || Startpoint == null || Endpoint == null) return;
        if (_eliteIndices.Count == 0) return;

        Vector3 start = Startpoint.transform.position; // 下端
        Vector3 end = Endpoint.transform.position;     // 上端
        Transform parent = ProgressBar ? ProgressBar.transform.parent : Startpoint.transform.parent;

        foreach (int idx in _eliteIndices)
        {
            float t = ProgressOfIndex(idx); // 例：120中30番目→ 0.25
            Vector3 pos = Vector3.Lerp(start, end, t);

            // UI前提：ワールド座標で位置合わせ（同Canvas内を想定）
            GameObject icon = Instantiate(WarningIcon, parent);
            var rt = icon.GetComponent<RectTransform>();
            if (rt != null)
            {
                // ワールド座標 → そのままposition（同Canvas/Screenspace前提）
                rt.position = pos;
            }
            else
            {
                icon.transform.position = pos;
            }
        }
    }

    // プログレスバー更新（KilledEnemyNum / enemyCreateLimit）
    void UpdateProgressBar()
    {
        if (!ProgressBar) return;

        float denom = Mathf.Max(1, enemyCreateLimit);
        float p = Mathf.Clamp01((float)KilledEnemyNum / denom);

        // Image(Type=Filled)前提
        ProgressBar.fillAmount = p;
    }

    // （任意）外部から呼ぶヘルパ：撃破時
    public void OnEnemyKilled()
    {
        KilledEnemyNum++;
        UpdateProgressBar();
    }
}
