public static class GameEvents
{
    public static System.Action OnDoStartGame;
    public static System.Action OnDoStopGame;

    public static System.Action OnNotifyProfileIsLoaded;
    public static System.Action OnDoProfileSave;


    public static void NotifyProfileIsLoaded()
    {
        if (OnNotifyProfileIsLoaded != null) OnNotifyProfileIsLoaded.Invoke();
    }


    public static void DoProfileSave()
    {
        if (OnDoProfileSave != null) OnDoProfileSave.Invoke();
    }

    
    public static void DoStartGame()
    {
        if (OnDoStartGame != null) OnDoStartGame.Invoke();
    }

    public static void DoStopGame()
    {
        if (OnDoStopGame != null) OnDoStopGame.Invoke();
    }
}