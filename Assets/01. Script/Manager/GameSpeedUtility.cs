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
            // 기본 속도
            case GameSpeedMode.Normal: return 1f;

            // 2배
            case GameSpeedMode.Fast: return 2f;

             // 3배
            case GameSpeedMode.VeryFast: return 3f;
            default: return 1f;
        }
    }
}
