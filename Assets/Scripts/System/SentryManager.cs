using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryManager : MonoBehaviour
{
   // public List<GameObject> sentryList = new List<GameObject>();//セントリーのリスト
    public List<GameObject> sentryList = new List<GameObject>(); // 管理するセントリー
    public Transform player; // プレイヤーのTransform（Inspectorでセット推奨）

    public float deployRadius = 3f;      // 展開時の半径
    public float rotationSpeed = 30f;    // 展開時の回転速度（度/秒）

    private float currentRotation = 0f;  // 現在の回転角度（展開時用）

    public enum SentryState { Idle, Gather, Deploy, Disperse, Follow }
    public SentryState currentState = SentryState.Idle;

    void Update()
    {
        HandleInput();

        switch (currentState)
        {
            case SentryState.Gather://集合
                GatherSentries();
                break;
            case SentryState.Deploy://展開
                DeploySentries();
                break;
            case SentryState.Disperse://散開　個別戦闘
                // 個別で動くのでここでは特に処理なし（各セントリーに任せる）
                break;
            case SentryState.Follow:
                FollowPlayer();
                break;
            case SentryState.Idle:
            default:
                // 何もしない
                break;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentState = SentryState.Gather;
            Debug.Log("セントリー: 集合");

            foreach (var s in sentryList)
            s.transform.SetParent(player.transform); // プレイヤーの子に

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentState = SentryState.Deploy;
            Debug.Log("セントリー: 展開");
            // 展開開始時の角度リセット
            //currentRotation = 0f;
            foreach (var s in sentryList)
            {
                s.transform.SetParent(player.transform);
                s.SetActive(true);
            }
                
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentState = SentryState.Disperse;
            Debug.Log("セントリー: 分散");
            foreach (var s in sentryList)
            {
                s.transform.SetParent(null);
              //  s.SetActive(false);//親子関係解除
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentState = SentryState.Follow;
            Debug.Log("セントリー: 追従");
            foreach (var s in sentryList)
            {
                s.transform.SetParent(player.transform);
                s.SetActive(true);
            }
        }
    }

    private void GatherSentries()
    {
        // 全セントリーをプレイヤーの位置に高速移動し、重なったら非アクティブ化
        foreach (var s in sentryList)
        {
            if (s == null) continue;

            Vector3 targetPos = player.position;
            // 超高速移動(線形補間に近いがほぼ瞬間移動に近づける)
            s.transform.position = Vector3.MoveTowards(s.transform.position, targetPos, 100f * Time.deltaTime);

            // プレイヤーに近づいたら非アクティブ化
            if (Vector3.Distance(s.transform.position, targetPos) < 0.1f)
            {
                s.SetActive(false);
            }
        }
    }

    private void DeploySentries()
    {
        int count = sentryList.Count;
        if (count == 0) return;

        currentRotation += rotationSpeed * Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            var s = sentryList[i];
            if (s == null) continue;

            float angle = currentRotation + (360f / count) * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * deployRadius;
            Vector3 targetPos = player.position + offset;

            // じわっと移動
            s.transform.position = Vector3.Lerp(s.transform.position, targetPos, 2f * Time.deltaTime);
        }
    }
    List<Vector3> playerPositions = new List<Vector3>();  // プレイヤーの軌跡（最新が最後）
    public int maxHistory = 100;        // 軌跡保持数の最大値
    public int delayStep = 5;            // セントリーごとの遅れ幅（軌跡の間隔）
    public float minSpeed = 2f;          // 最低移動速度（プレイヤーに近いとき）
    public float maxSpeed = 10f;         // 最大移動速度（プレイヤーから遠いとき）
    public float maxFollowDistance = 5f; // 速度計算の基準距離（これを超えたらmaxSpeed）
    private void FollowPlayer()
    {
        if (player == null || sentryList.Count == 0) return;

        // プレイヤーの現在位置を軌跡に追加
        playerPositions.Add(player.position);
        if (playerPositions.Count > maxHistory) playerPositions.RemoveAt(0);

        // 各セントリーに目標位置を割り当てる（後ろのほど遅れた位置）
        for (int i = 0; i < sentryList.Count; i++)
        {
            int targetIndex = Mathf.Clamp(playerPositions.Count - 1 - i * delayStep, 0, playerPositions.Count - 1);
            Vector3 targetPos = playerPositions[targetIndex];

            var s = sentryList[i];
            if (s == null) continue;

            float distance = Vector3.Distance(s.transform.position, targetPos);

            // 距離に応じて速度調整（距離が大きいほど速く）
            float speed = Mathf.Lerp(minSpeed, maxSpeed, Mathf.Clamp01(distance / maxFollowDistance));

            // 追い越し防止のため、前のセントリーとの距離を確認して調整可能
            if (i > 0)
            {
                float distToPrev = Vector3.Distance(s.transform.position, sentryList[i - 1].transform.position);
                if (distToPrev < delayStep * 0.5f)
                {
                    // 近すぎたら速度抑制
                    speed = Mathf.Min(speed, 1f);
                }
            }

            s.transform.position = Vector3.MoveTowards(s.transform.position, targetPos, speed * Time.deltaTime);
        }
    }
}
