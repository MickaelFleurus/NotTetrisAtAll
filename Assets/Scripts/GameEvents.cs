using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

class GameEvents
{
    public static void CloseGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // In WebGL, just exit fullscreen
        if (Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }
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
        await Task.Delay(500);
        SceneManager.LoadScene("GameScene");
    }

    public static void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
