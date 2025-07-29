using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigatorFloat : MonoBehaviour
{
    public Transform player;
    public float minDistance = 1.0f;
    public float maxDistance = 2.0f;
    public float minMoveDuration = 0.5f;
    public float maxMoveDuration = 2.0f;
    public float moveSpeed = 5f;

    private Vector3 currentOffset; // プレイヤーからの相対座標
    private float moveDuration;
    private float moveTimer;

    void Start()
    {
        ChooseNewTarget();
    }

    void Update()
    {
        if (player == null) return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveDuration)
        {
            ChooseNewTarget();
        }

        // プレイヤー基準の目標ワールド座標
        Vector3 targetPos = player.position + currentOffset;

        // 滑らかに補間移動
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }

    void ChooseNewTarget()
    {
        moveTimer = 0f;
        moveDuration = Random.Range(minMoveDuration, maxMoveDuration);

        // ドーナツ範囲内のランダム相対座標
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minDistance, maxDistance);
        currentOffset = new Vector3(randomDir.x, randomDir.y, 0f) * randomDist;
    }
}
