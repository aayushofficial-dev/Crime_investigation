using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour  // ← change SceneChanger to LevelLoader
{
    public void GoToNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }
}