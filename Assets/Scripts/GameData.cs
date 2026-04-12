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

    public EGameMode gameMode { get; private set; } = EGameMode.Marathon;
    public int levelStart { get; private set; } = 0;
    public int blockSize { get; private set; } = 4;
    public EGameTimeLimit timeLimit { get; private set; } = EGameTimeLimit.One;


    public void OnGameStarted(EGameMode gameMode, int levelStart, int blockSize, EGameTimeLimit timeLimit)
    {
        this.gameMode = gameMode;
        this.levelStart = levelStart;
        this.blockSize = blockSize;
        this.timeLimit = timeLimit;
    }
}
