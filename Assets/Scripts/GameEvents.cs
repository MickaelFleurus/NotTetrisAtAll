using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

class GameEvents
{
    public static void CloseGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
    // Do Nothing
#else
    Application.Quit();
#endif
    }

    public static void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public static async void StartGame(EGameMode gameMode, int levelStart, int blockSize, EGameTimeLimit timeLimit)
    {
        GameData.Instance.OnGameStarted(gameMode, levelStart, blockSize, timeLimit);
#if UNITY_WEBGL
        await SceneManager.LoadSceneAsync("GameScene");
#else
        await Task.Delay(500);
        SceneManager.LoadScene("GameScene");
#endif
    }

    public static void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
