using UnityEngine;

class GameData
{

    private static GameData instance;
    public static GameData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameData();
            }
            return instance;
        }
    }

    public class GameOverData
    {
        public enum EGameOverReason { Lost, TimeUp };
        public EGameOverReason gameOverReason;
        public int finalScore;
        public int lineCompleted;
        public Timer finalTimer;
        public int finalLevel;
    }

    public EGameMode gameMode { get; private set; } = EGameMode.Marathon;
    public int levelStart { get; private set; } = 0;
    public int blockSize { get; private set; } = 4;
    public EGameTimeLimit timeLimit { get; private set; } = EGameTimeLimit.One;
    public GameOverData gameOverData = null;


    public void OnGameStarted(EGameMode gameMode, int levelStart, int blockSize, EGameTimeLimit timeLimit)
    {
        this.gameMode = gameMode;
        this.levelStart = levelStart;
        this.blockSize = blockSize;
        this.timeLimit = timeLimit;
    }


}
