using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LevelUpSelectionUI : MonoBehaviour
{
    public GameObject playerObj;
    public TextMeshProUGUI[] PLevelsText;//プレイヤーのレベル表示するテキスト

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       

    }
    public void UpdateLevelTexts()//UI更新　レベルアップ時に呼ぶ
    {
        if (playerObj == null) return;

        var player = playerObj.GetComponent<Player>();
        for (int i = 0; i < PLevelsText.Length; i++)
        {
            if (PLevelsText[i] != null)
            {
                int currentLevel = player.levels[i];
                PLevelsText[i].text = $"{currentLevel}LV → {currentLevel + 1}LV";
            }
        }
    }

    public void OnUpgradeButton(int index)
    {
        if (playerObj != null)
        {
            playerObj.GetComponent<Player>().levels[index]++;
        }
        playerObj.GetComponent<Player>().totalLevel++;//レベル上げる
        Time.timeScale = 1f;
        playerObj.GetComponent<Player>().LevelUpUI.SetActive(false);//非表示

    }
    public void OnHealSentries()//全体回復
    {
        var sentryManager = playerObj.GetComponent<SentryManager>();
        if (sentryManager == null) return;

        foreach (var sentry in sentryManager.sentryList)
        {
            if (sentry == null) continue;

            var sb = sentry.GetComponent<SentryBase>();
            if (sb == null) continue;

            // 5割回復（最大HPの50%を回復）
            sb.HP = Mathf.Min(sb.HP + sb.MaxHP * 0.5f, sb.MaxHP);
        }
        Time.timeScale = 1f;
        playerObj.GetComponent<Player>().LevelUpUI.SetActive(false);
    }

}
