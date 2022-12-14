using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    //Class for managing the levels
    private UIHandler uiHandler;
    private Grapple grappleObj;
    private EndlessLevel endless;
    private MainMenu mainMenu;
    private Player player;
    private string currentLevelName;
    private Level currentLevel;
    private AudioSource musicSource;

    public LevelManager(Grapple grappleObject, Player playerObj, UIHandler uiHandlerObj)
    {
        uiHandler = uiHandlerObj;
        player = playerObj;
        grappleObj = grappleObject;
        musicSource = GameObject.Find("MusicSource").GetComponent<AudioSource>();
        currentLevelName = SceneManager.GetActiveScene().name;
    }

    public void LevelManagerStart()
    {
        switch (currentLevelName)
        {
            case "Menu":
                mainMenu = new MainMenu();
                mainMenu.LevelStart();
                currentLevel = mainMenu;
                break;
            case "Endless":
                endless = new EndlessLevel(grappleObj, player, uiHandler);
                endless.LevelStart();
                currentLevel = endless;
                break;
        }
        //Creates a UIHandler only if the scene is not the menu
        //Otherwise would cause a null reference crash
        if (SceneManager.GetActiveScene().name != "Menu")
        {
            uiHandler.LevelRef(currentLevel);
        }
    }

    public void LevelManagerUpdate()
    {
        currentLevelName = SceneManager.GetActiveScene().name;
        currentLevel.LevelUpdate();
        UpdateVolume();
    }

    //Updating the volume values
    public void UpdateVolume()
    {
        AudioListener.volume = PlayerPrefs.GetInt("effectsVolume") / 10f;
        musicSource.volume = PlayerPrefs.GetInt("musicVolume") / 10f;
    }

    //Getter for currently running level
    public Level GetCurrentLevel()
    {
        return currentLevel;
    }
}
