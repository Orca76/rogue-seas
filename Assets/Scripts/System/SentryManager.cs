using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryManager : MonoBehaviour
{
   // public List<GameObject> sentryList = new List<GameObject>();//�Z���g���[�̃��X�g
    public List<GameObject> sentryList = new List<GameObject>(); // �Ǘ�����Z���g���[
    public Transform player; // �v���C���[��Transform�iInspector�ŃZ�b�g�����j

    public float deployRadius = 3f;      // �W�J���̔��a
    public float rotationSpeed = 30f;    // �W�J���̉�]���x�i�x/�b�j

    private float currentRotation = 0f;  // ���݂̉�]�p�x�i�W�J���p�j

    public enum SentryState { Idle, Gather, Deploy, Disperse, Follow }
    public SentryState currentState = SentryState.Idle;

    void Update()
    {
        HandleInput();

        switch (currentState)
        {
            case SentryState.Gather://�W��
                GatherSentries();
                break;
            case SentryState.Deploy://�W�J
                DeploySentries();
                break;
            case SentryState.Disperse://�U�J�@�ʐ퓬
                // �ʂœ����̂ł����ł͓��ɏ����Ȃ��i�e�Z���g���[�ɔC����j
                break;
            case SentryState.Follow:
                FollowPlayer();
                break;
            case SentryState.Idle:
            default:
                // �������Ȃ�
                break;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentState = SentryState.Gather;
            Debug.Log("�Z���g���[: �W��");

            foreach (var s in sentryList)
            s.transform.SetParent(player.transform); // �v���C���[�̎q��

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentState = SentryState.Deploy;
            Debug.Log("�Z���g���[: �W�J");
            // �W�J�J�n���̊p�x���Z�b�g
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
            Debug.Log("�Z���g���[: ���U");
            foreach (var s in sentryList)
            {
                s.transform.SetParent(null);
              //  s.SetActive(false);//�e�q�֌W����
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentState = SentryState.Follow;
            Debug.Log("�Z���g���[: �Ǐ]");
            foreach (var s in sentryList)
            {
                s.transform.SetParent(player.transform);
                s.SetActive(true);
            }
        }
    }

    private void GatherSentries()
    {
        // �S�Z���g���[���v���C���[�̈ʒu�ɍ����ړ����A�d�Ȃ������A�N�e�B�u��
        foreach (var s in sentryList)
        {
            if (s == null) continue;

            Vector3 targetPos = player.position;
            // �������ړ�(���`��Ԃɋ߂����قڏu�Ԉړ��ɋ߂Â���)
            s.transform.position = Vector3.MoveTowards(s.transform.position, targetPos, 100f * Time.deltaTime);

            // �v���C���[�ɋ߂Â������A�N�e�B�u��
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

            // ������ƈړ�
            s.transform.position = Vector3.Lerp(s.transform.position, targetPos, 2f * Time.deltaTime);
        }
    }
    List<Vector3> playerPositions = new List<Vector3>();  // �v���C���[�̋O�Ձi�ŐV���Ō�j
    public int maxHistory = 100;        // �O�Օێ����̍ő�l
    public int delayStep = 5;            // �Z���g���[���Ƃ̒x�ꕝ�i�O�Ղ̊Ԋu�j
    public float minSpeed = 2f;          // �Œ�ړ����x�i�v���C���[�ɋ߂��Ƃ��j
    public float maxSpeed = 10f;         // �ő�ړ����x�i�v���C���[���牓���Ƃ��j
    public float maxFollowDistance = 5f; // ���x�v�Z�̊�����i����𒴂�����maxSpeed�j
    private void FollowPlayer()
    {
        if (player == null || sentryList.Count == 0) return;

        // �v���C���[�̌��݈ʒu���O�Ղɒǉ�
        playerPositions.Add(player.position);
        if (playerPositions.Count > maxHistory) playerPositions.RemoveAt(0);

        // �e�Z���g���[�ɖڕW�ʒu�����蓖�Ă�i���̂قǒx�ꂽ�ʒu�j
        for (int i = 0; i < sentryList.Count; i++)
        {
            int targetIndex = Mathf.Clamp(playerPositions.Count - 1 - i * delayStep, 0, playerPositions.Count - 1);
            Vector3 targetPos = playerPositions[targetIndex];

            var s = sentryList[i];
            if (s == null) continue;

            float distance = Vector3.Distance(s.transform.position, targetPos);

            // �����ɉ����đ��x�����i�������傫���قǑ����j
            float speed = Mathf.Lerp(minSpeed, maxSpeed, Mathf.Clamp01(distance / maxFollowDistance));

            // �ǂ��z���h�~�̂��߁A�O�̃Z���g���[�Ƃ̋������m�F���Ē����\
            if (i > 0)
            {
                float distToPrev = Vector3.Distance(s.transform.position, sentryList[i - 1].transform.position);
                if (distToPrev < delayStep * 0.5f)
                {
                    // �߂������瑬�x�}��
                    speed = Mathf.Min(speed, 1f);
                }
            }

            s.transform.position = Vector3.MoveTowards(s.transform.position, targetPos, speed * Time.deltaTime);
        }
    }
}
