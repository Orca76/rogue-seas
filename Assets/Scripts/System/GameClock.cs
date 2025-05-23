using UnityEngine;

public class GameClock : MonoBehaviour
{
    public int hour = 0;     // 現在の時刻（時）
    public int minute = 0;   // 現在の時刻（分）

    public float timeSpeed = 2f; // 1秒で何分進めるか（初期値2 = 6分で1周）

    private float timer = 0f;

    public delegate void TimeChangedDelegate(int hour, int minute);
    public event TimeChangedDelegate OnTimeChanged;

    void Update()
    {
        timer += Time.deltaTime * timeSpeed;

        if (timer >= 1f)
        {
            int minutesToAdd = Mathf.FloorToInt(timer);
            timer -= minutesToAdd;
            AddMinutes(minutesToAdd);
        }
    }

    public void AddMinutes(int mins)
    {
        minute += mins;
        while (minute >= 60)
        {
            minute -= 60;
            hour = (hour + 1) % 24;
        }

        OnTimeChanged?.Invoke(hour, minute);
    }

    public float GetAngleForCurrentTime()
    {
        float totalMinutes = hour * 60 + minute;
        return (totalMinutes / 1440f) * 360f;
    }
}
