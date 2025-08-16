using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetSail : MonoBehaviour
{
    [Header("関連UI")]
    public GameObject ChartUI;           // 開く対象の錬成UI（非アクティブ状態から切り替え）
    public KeyCode interactKey = KeyCode.E; // インタラクト用キー（例：Eキー）

    private bool playerInsea = true;     // プレイヤーが範囲内にいるかどうか

    GameObject InventoryUIObj;
    GameObject CreateUIObj;
    public ChartVectorManager chartSystem;//ベクトル出してる奴　リセットの為に呼ぶ

    GameObject VectorSystem;//上の奴の実体
    public GameObject FieldText;//何が生成されるかのテキスト






    GameObject player;
    public int NextDestination;//次の目的地



    GameObject map;//錬成盤リロード用　マップデータ
    AlchemyTile tiledata;
    private void Start()
    {
        RebindIfNeeded();  // 初回
    }

    // SetSail 内に追加：必要な参照だけ再取得するミニ関数
    void RebindIfNeeded()
    {
        if (!VectorSystem) VectorSystem = GameObject.Find("VectorManager");
        if (!chartSystem && VectorSystem) chartSystem = VectorSystem.GetComponent<ChartVectorManager>();

        if (!player) player = GameObject.FindWithTag("Player");

        if (!map) map = GameObject.Find("map");
        if (!tiledata && map) tiledata = map.GetComponent<AlchemyTile>();

        if (!InventoryUIObj) InventoryUIObj = GameObject.Find("HotbarPanel");
        // FieldText も名前決まってるなら同様に:
        // if (!FieldText) FieldText = GameObject.Find("FieldText");
    }
    private void Update()
    {
        // 範囲内にプレイヤーがいて、かつインタラクトキーが押されたらUIを開く
        if (playerInsea && Input.GetKeyDown(interactKey))
        {
            OpenChartUI();
        }




    }

    // UIを開く処理
    private void OpenChartUI()
    {
        RebindIfNeeded();
        ChartUI.SetActive(true);
        Time.timeScale = 0f;   // ゲーム全体を停止

        chartSystem.RegionText = FieldText.GetComponent<TextMeshProUGUI>(); ;//テキストを今出てるクリスタルのものに指定
        if (InventoryUIObj == null)
        {
            InventoryUIObj = GameObject.Find("HotbarPanel");//起動時にインベントリ取得
        }

        //インベントリの位置を上に
        SetPosY(30);
        tiledata.CreationTile(GetSeedFromCoords(gameObject.transform.position.x, gameObject.transform.position.y));
        Debug.Log("Alchemy UI opened");
    }
    int GetSeedFromCoords(float x, float y)//座標から乱数生成
    {
        int xi = Mathf.FloorToInt(x * 1000f);  // 必要ならスケーリング
        int yi = Mathf.FloorToInt(y * 1000f);
        return xi * 1000000 + yi; // xの桁を広げて足すだけ
    }

    public void CloseChartUI()//戻るボタン　UIを閉じる
    {
        //インベントリの位置を下に
        SetPosY(-100);
        chartSystem.ResetVectors();//閉じたらベクトルリセット
        Time.timeScale = 1f;   // 元に戻す

        ChartUI.SetActive(false);
        Debug.Log("Chart UI closed");
    }

    void SetPosY(float y)//インベントリの位置変更
    {
        var rect = InventoryUIObj.GetComponent<RectTransform>();
        var pos = rect.anchoredPosition;
        pos.y = y;
        rect.anchoredPosition = pos;
    }
    public void GoNextIsland()//出航ボタンを押した時
    {
        NextDestination = chartSystem.zoneType;//次の島に行く時に必要


        List<GameObject> S = player.GetComponent<SentryManager>().sentryList;

        foreach (GameObject sentry in S)//セントリーを全部子オブジェクトに（次の島に連れていく）
        {
            // nullチェックも一応
            if (sentry != null)
            {
                sentry.transform.SetParent(player.transform);
            }
        }

        DontDestroyOnLoad(player);//プレイヤーは保持
        Player pobj=player.GetComponent<Player>();
        pobj.NextDest = NextDestination;
        pobj.VisitedIslandCount++;
        CloseChartUI();//UIは閉じる


        SceneManager.LoadScene("SampleScene");//新しい島へ
     
       

    }

}

