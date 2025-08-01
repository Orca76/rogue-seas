using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LevelUpSelectionUI : MonoBehaviour
{
    public GameObject playerObj;
    public TextMeshProUGUI[] PLevelsText;//�v���C���[�̃��x���\������e�L�X�g

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       

    }
    public void UpdateLevelTexts()//UI�X�V�@���x���A�b�v���ɌĂ�
    {
        if (playerObj == null) return;

        var player = playerObj.GetComponent<Player>();
        for (int i = 0; i < PLevelsText.Length; i++)
        {
            if (PLevelsText[i] != null)
            {
                int currentLevel = player.levels[i];
                PLevelsText[i].text = $"{currentLevel}LV �� {currentLevel + 1}LV";
            }
        }
    }

    public void OnUpgradeButton(int index)
    {
        if (playerObj != null)
        {
            playerObj.GetComponent<Player>().levels[index]++;
        }
        playerObj.GetComponent<Player>().totalLevel++;//���x���グ��
        Time.timeScale = 1f;
        playerObj.GetComponent<Player>().LevelUpUI.SetActive(false);//��\��

    }
    public void OnHealSentries()//�S�̉�
    {
        var sentryManager = playerObj.GetComponent<SentryManager>();
        if (sentryManager == null) return;

        foreach (var sentry in sentryManager.sentryList)
        {
            if (sentry == null) continue;

            var sb = sentry.GetComponent<SentryBase>();
            if (sb == null) continue;

            // 5���񕜁i�ő�HP��50%���񕜁j
            sb.HP = Mathf.Min(sb.HP + sb.MaxHP * 0.5f, sb.MaxHP);
        }
        Time.timeScale = 1f;
        playerObj.GetComponent<Player>().LevelUpUI.SetActive(false);
    }

}
