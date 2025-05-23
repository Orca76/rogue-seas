using UnityEngine;

public class GameClock : MonoBehaviour
{
    public int hour = 0;     // ���݂̎����i���j
    public int minute = 0;   // ���݂̎����i���j

    public float timeSpeed = 2f; // 1�b�ŉ����i�߂邩�i�����l2 = 6����1���j

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
