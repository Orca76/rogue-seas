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

    private Vector3 currentOffset; // �v���C���[����̑��΍��W
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

        // �v���C���[��̖ڕW���[���h���W
        Vector3 targetPos = player.position + currentOffset;

        // ���炩�ɕ�Ԉړ�
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }

    void ChooseNewTarget()
    {
        moveTimer = 0f;
        moveDuration = Random.Range(minMoveDuration, maxMoveDuration);

        // �h�[�i�c�͈͓��̃����_�����΍��W
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minDistance, maxDistance);
        currentOffset = new Vector3(randomDir.x, randomDir.y, 0f) * randomDist;
    }
}
