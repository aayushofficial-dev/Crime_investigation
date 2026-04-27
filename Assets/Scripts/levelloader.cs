using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void GoToNextScene()
    {
        SceneManager.LoadScene("material07");
    }
}