using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    } 
    public static void FreeLoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
