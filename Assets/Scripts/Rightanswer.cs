using UnityEngine;
using UnityEngine.SceneManagement;

public class Rightanswer : MonoBehaviour
{
    public void GoToNextScene()
    {
        SceneManager.LoadScene("correctanswer");
    }
}