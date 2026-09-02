using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Inställningar")]
    public string firstLevelName = "GameScene"; // Namnet på din spelnivå i Build Settings

    public GameObject MainMenu_;
    public GameObject TextDisable;
    Animator anime;


    void Start()
    {
        anime = GetComponent<Animator>();
    }
    void Update()
    {
        // Tryck på Enter, Space, Escape eller Start på handkontroll för att starta
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MainMenuLoad();
             // Dölj texten när menyn visas
        }
    }

    public void MainMenuLoad()
    {
        anime.enabled = false;
        TextDisable.SetActive(false);
        MainMenu_.SetActive(true);
    }

    public void LoadGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(firstLevelName);
    }

    public void QuitGame()
    {
        Debug.Log("Spelet stängs av...");
        Application.Quit();
    }
}