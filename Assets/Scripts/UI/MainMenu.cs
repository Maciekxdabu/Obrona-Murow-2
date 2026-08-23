using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName;

    // ---------- public Menu methods

    public void OnStartGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnExitGame()
    {
        Application.Quit();
    }
}
