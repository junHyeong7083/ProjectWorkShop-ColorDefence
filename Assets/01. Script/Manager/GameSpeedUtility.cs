public enum GameSpeedMode
{
    Normal = 1,
    Fast = 2,
    VeryFast = 3
}

public static class GameSpeedUtility
{
    public static float GetTimeScale(GameSpeedMode mode)
    {
        switch (mode)
        {
            // �⺻ �ӵ�
            case GameSpeedMode.Normal: return 1f;

            // 2��
            case GameSpeedMode.Fast: return 2f;

             // 3��
            case GameSpeedMode.VeryFast: return 3f;
            default: return 1f;
        }
    }
}
