public static class GameEvents
{
    public static System.Action OnStartGame;
    public static System.Action OnStopGame;

    public static void StartGame()
    {
        if (OnStartGame != null) OnStartGame.Invoke();
    }

    public static void StopGame()
    {
        if (OnStopGame != null) OnStopGame.Invoke();
    }
}