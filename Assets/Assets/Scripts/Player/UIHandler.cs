using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UIHandler : MonoBehaviour
{
    //Pause Menu Variables
    private GameObject pauseScreen;
    private GameObject resumeUI, restartUI, optionsUI, exitUI, pauseMenuHolder;
    private GameObject[] pauseMenuArray;
    private Image[] pauseMenuImageArray;

    //Confirm Prompt Variables
    private GameObject yesUI, noUI, confirmPromptHolder;
    private GameObject[] confirmPromptArray;
    private Image[] confirmPromptImageArray;

    //Options Menu Variables
    private GameObject effectVolumeUI, musicVolumeUI, mouseSensUI, backUI, optionsHolder;
    private GameObject[] optionsArray;
    private Image[] optionsImageArray;
    private string effectVolString, musicVolString, mouseSensString;
    private Text effectVolText, musicVolText, mouseSensText;
    private int effectVolume, musicVolume;
    private float mouseSens;
    private float mouseSensIncrement = 0.05f;

    //Endless Level Score & Confirm
    private GameObject endlessYesUI, endlessNoUI, endlessConfirmHolder;
    private GameObject[] endlessConfirmArray;
    private Image[] endlessConfirmImageArray;

    //General Variables
    private Level currentLevel;
    private GameObject[] holderArray;
    private GameObject[] currentArray;
    private Image[] currentImageArray;
    private GameObject selectedElement;
    private LevelManager levelManager;
    private Player player;
    private Grapple grapple;
    private int selectedElemNum = 0;
    private Color32 selectedColor = new Color32(255, 255, 225, 225);
    private Color32 fadedColor = new Color32(100, 100, 100, 225);
    private bool isPaused = false;
    private string confirmType;

    //Audio Variables
    private AudioSource menuClick;
    private AudioSource menuConfirm;
    private AudioSource menuBack;

    //Variables for Timed Hold Input
    private float startTime;
    private float holdTime = 1f;
    private bool playerHasReset = false;

    public UIHandler(Player playerObj, Grapple grappleObj)
    {
        //Pause Menu
        pauseScreen = GameObject.Find("PauseMenu");
        pauseMenuHolder = GameObject.Find("PauseMenuHolder");
        resumeUI = GameObject.Find("Resume");
        restartUI = GameObject.Find("Restart");
        optionsUI = GameObject.Find("Options");
        exitUI = GameObject.Find("Exit");
        pauseMenuArray = new GameObject[] { resumeUI, restartUI, optionsUI, exitUI };
        pauseMenuImageArray = new Image[] { resumeUI.GetComponent<Image>(), restartUI.GetComponent<Image>(), optionsUI.GetComponent<Image>(), exitUI.GetComponent<Image>() };

        //Confirm Prompt
        confirmPromptHolder = GameObject.Find("ConfirmPrompt");
        yesUI = GameObject.Find("Yes");
        noUI = GameObject.Find("No");
        confirmPromptArray = new GameObject[] { yesUI, noUI};
        confirmPromptImageArray = new Image[] { yesUI.GetComponent<Image>(), noUI.GetComponent<Image>()};

        //Options Menu
        optionsHolder = GameObject.Find("OptionsHolder");
        effectVolumeUI = GameObject.Find("Effects Volume");
        musicVolumeUI = GameObject.Find("Music Volume");
        mouseSensUI = GameObject.Find("Mouse Sensitivity");
        backUI = GameObject.Find("Back");
        optionsArray = new GameObject[] { effectVolumeUI, musicVolumeUI, mouseSensUI, backUI };
        optionsImageArray = new Image[] { effectVolumeUI.GetComponent<Image>(), musicVolumeUI.GetComponent<Image>(), mouseSensUI.GetComponent<Image>(), backUI.GetComponent<Image>() };

        //Music & Effects Volume
        effectVolText = GameObject.Find("EffectsVolValue").GetComponent<Text>();
        musicVolText = GameObject.Find("MusicVolValue").GetComponent<Text>();
        mouseSensText = GameObject.Find("MouseSensValue").GetComponent<Text>();

        //Audio
        menuClick = GameObject.Find("MenuMove").GetComponent<AudioSource>();
        menuConfirm = GameObject.Find("MenuConfirm").GetComponent<AudioSource>();
        menuBack = GameObject.Find("MenuBack").GetComponent<AudioSource>();

        //Endless Score Box
        if (SceneManager.GetActiveScene().name == "Endless")
        {
            endlessConfirmHolder = GameObject.Find("Endless Scored Box");
            endlessYesUI = GameObject.Find("EndlessYes");
            endlessNoUI = GameObject.Find("EndlessNo");
            endlessConfirmArray = new GameObject[] { endlessYesUI, endlessNoUI };
            endlessConfirmImageArray = new Image[] { endlessYesUI.GetComponent<Image>(), endlessNoUI.GetComponent<Image>() };
            holderArray = new GameObject[] { pauseMenuHolder, confirmPromptHolder, optionsHolder, endlessConfirmHolder };
        }
        else
            holderArray = new GameObject[] { pauseMenuHolder, confirmPromptHolder, optionsHolder };
        selectedElement = resumeUI;
        player = playerObj;
        grapple = grappleObj;
    }

    public void UIStart()
    {
        if (!PlayerPrefs.HasKey("effectsVolume"))
            PlayerPrefs.SetInt("effectsVolume", 5);
        if (!PlayerPrefs.HasKey("musicVolume"))
            PlayerPrefs.SetInt("musicVolume", 5);
        currentArray = pauseMenuArray;
        currentImageArray = pauseMenuImageArray;
        SetMenuSelection(pauseMenuHolder, pauseMenuArray, pauseMenuImageArray);
    }

    public void UIUpdate()
    {
        PauseCheck();
        ResetCheck();
    }

    //Simplified logic of the main menu select, without the animation
    public void PauseMenuSelection()
    {
        if (!currentArray[0] || !currentImageArray[0])
            return;
        for (int i = 0; i < currentArray.Length; i++)
        {
            if (i == selectedElemNum)
            {
                selectedElement = currentArray[i];
                currentImageArray[i].color = selectedColor;
            }
            else
                currentImageArray[i].color = fadedColor;
        }

        //Input for changing the volume in the options screen
        if (currentArray == optionsArray)
        {
            if (Input.GetKeyDown("right") && selectedElement == effectVolumeUI && effectVolume < 10)
            {
                menuClick.Play();
                effectVolume++;
                effectVolText.text = effectVolume.ToString();
                PlayerPrefs.SetInt("effectsVolume", effectVolume);
            }
            if (Input.GetKeyDown("left") && selectedElement == effectVolumeUI && effectVolume > 0)
            {
                menuClick.Play();
                effectVolume--;
                effectVolText.text = effectVolume.ToString();
                PlayerPrefs.SetInt("effectsVolume", effectVolume);
            }
            if (Input.GetKeyDown("right") && selectedElement == musicVolumeUI && musicVolume < 10)
            {
                menuClick.Play();
                musicVolume++;
                musicVolText.text = musicVolume.ToString();
                PlayerPrefs.SetInt("musicVolume", musicVolume);
            }
            if (Input.GetKeyDown("left") && selectedElement == musicVolumeUI && musicVolume > 0)
            {
                menuClick.Play();
                musicVolume--;
                musicVolText.text = musicVolume.ToString();
                PlayerPrefs.SetInt("musicVolume", musicVolume);
            }
            if (Input.GetKeyDown("right") && selectedElement == mouseSensUI && mouseSens < 1.0f)
            {
                menuClick.Play();
                mouseSens += mouseSensIncrement;

                // Clamping mouse sens
                if (mouseSens >= 1.0f)
                {
                    mouseSens = 1.0f;
                }

                mouseSens = (float)Math.Round(mouseSens * 100f) / 100f;

                mouseSensText.text = mouseSens.ToString();
                PlayerPrefs.SetFloat("mouseSens", mouseSens);
            }
            if (Input.GetKeyDown("left") && selectedElement == mouseSensUI && mouseSens > 0.05f)
            {
                menuClick.Play();
                mouseSens -= mouseSensIncrement;

                // Clamping mouse sens
                if (mouseSens <= 0.05f)
                {
                    mouseSens = 0.05f;
                }

                mouseSens = (float)Math.Round(mouseSens * 100f) / 100f;

                mouseSensText.text = mouseSens.ToString();
                PlayerPrefs.SetFloat("mouseSens", mouseSens);
            }
        }

        if (Input.GetKeyDown("up") && selectedElemNum > 0)
        {
            menuClick.Play();
            selectedElemNum--;
        }

        if (Input.GetKeyDown("down") && selectedElemNum < currentArray.Length - 1)
        {
            menuClick.Play();
            selectedElemNum++;
        }

        //Conditonal for player confirming a menu selection
        if (Input.GetButtonDown("Confirm"))
        {
            //Unpauses the game
            if (selectedElement == resumeUI)
            {
                menuConfirm.Play();
                isPaused = false;
            }
            //Sends the player to the confirm prompt, where selecting yes will restart
            if (selectedElement == restartUI)
            {
                menuConfirm.Play();
                SetMenuSelection(confirmPromptHolder, confirmPromptArray, confirmPromptImageArray);
                confirmType = "Restart";
            }
            //Options menu, gets the volume variables before loading to the menu
            if (selectedElement == optionsUI)
            {
                menuConfirm.Play();
                effectVolume = PlayerPrefs.GetInt("effectsVolume");
                musicVolume = PlayerPrefs.GetInt("musicVolume");
                mouseSens = PlayerPrefs.GetFloat("mouseSens");
                effectVolText.text = effectVolume.ToString();
                musicVolText.text = musicVolume.ToString();
                mouseSensText.text = mouseSens.ToString();
                SetMenuSelection(optionsHolder, optionsArray, optionsImageArray);
            }
            //Exit condition which sends the player to a confirm prompt, where yes will return to main menu
            if (selectedElement == exitUI)
            {
                menuConfirm.Play();
                SetMenuSelection(confirmPromptHolder, confirmPromptArray, confirmPromptImageArray);
                confirmType = "Exit";
            }
            //Yes and No confirm prompts
            if (selectedElement == yesUI)
            {
                menuConfirm.Play();
                if (confirmType == "Restart")
                {
                    currentLevel.ResetLevel();
                    isPaused = false;
                }
                else if (confirmType == "Exit")
                    SceneManager.LoadScene("Menu");
            }
            if (selectedElement == noUI)
            {
                menuConfirm.Play();
                SetMenuSelection(pauseMenuHolder, pauseMenuArray, pauseMenuImageArray);
            }
            //Returns to the main pause menu
            if (selectedElement == backUI)
            {
                menuBack.Play();
                SetMenuSelection(pauseMenuHolder, pauseMenuArray, pauseMenuImageArray);
                PlayerPrefs.Save();
            }

            //Conditionals for the prompt that appears when you fail asking the player if they'd like to restart
            if (selectedElement == endlessYesUI)
            {
                menuConfirm.Play();
                grapple.StopGrapple("Both");
                player.SafePlayerReset();
                currentLevel.ResetLevel();
                isPaused = false;
                SetMenuSelection(pauseMenuHolder, pauseMenuArray, pauseMenuImageArray);
            }
            if (selectedElement == endlessNoUI)
            {
                menuConfirm.Play();
                SceneManager.LoadScene("Menu");
            }
        }
    }

    //Method for checking if the player has reset to stop constantly resetting on key hold.
    public void ResetCheck()
    {
        if (!isPaused)
        {
            if (Input.GetButtonDown("Reset"))
            {
                startTime = Time.time;
                playerHasReset = false;
            }
            if (Input.GetButton("Reset") && !playerHasReset)
            {
                if (startTime + holdTime <= Time.time)
                {
                    currentLevel.ResetLevel();
                    playerHasReset = true;
                }
            }
            if (Input.GetButtonUp("Reset"))
                playerHasReset = false;
        }
    }

    //Method for checking if the player has pressed the pause key, and if so, runs the code for the pause menu, using Time.timeScale to pause physics
    public void PauseCheck()
    {
        if (Input.GetButtonDown("Pause") && currentArray != endlessConfirmArray)
        {
            menuClick.Play();
            SetMenuSelection(pauseMenuHolder, pauseMenuArray, pauseMenuImageArray);
            if (isPaused)
            {
                player.SafePlayerReset();
                grapple.StopGrapple("Both");
            }
            isPaused = !isPaused;
        }
        if (isPaused)
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PauseMenuSelection();
        }
        else if (!isPaused)
        {
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    //Nearly identical to Main Menu version of this method, sets the next menu to display
    public void SetMenuSelection(GameObject menuToEnable, GameObject[] arrayToSwitch, Image[] imageArrayToSwitch)
    {
        for (int i = 0; i < holderArray.Length; i++)
        {
            holderArray[i].SetActive(false);
        }

        menuToEnable.SetActive(true);
        currentArray = arrayToSwitch;
        currentImageArray = imageArrayToSwitch;
        selectedElemNum = 0;
    }

    //Method for displaying the confirm prompt to the player after failing
    public void EndlessConfirmBox()
    {
        SetMenuSelection(endlessConfirmHolder, endlessConfirmArray, endlessConfirmImageArray);
        isPaused = true;
    }

    //Getters and Setters
    public void LevelRef(Level currentLevelRef)
    {
        currentLevel = currentLevelRef;
    }

    public bool GetPauseState()
    {
        return isPaused;
    }
}
