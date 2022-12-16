using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MainMenu : Level
{
    //Main Menu Variables
    private GameObject howToUI, playUI, optionsUI, creditsUI, backPromptUI, menuHolder;
    private GameObject[] menuArray;
    private Image[] menuImageArray;

    //Options Variables
    private GameObject effectVolumeUI, musicVolumeUI, mouseSensUI, optionsHolder;
    private GameObject[] optionsArray;
    private Image[] optionsImageArray;
    private string effectVolString, musicVolString;
    private Text effectVolText, musicVolText, mouseSensText;
    private int effectVolume, musicVolume;
    private float mouseSens;
    private float mouseSensIncrement = 0.05f;

    //Flavour Text Variables
    private GameObject flavourTextHolder;
    private Text flavourText;
    //Main Menu
    private string howToFlavour = "Learn how to play DefNet.";
    private string playFlavour = "Start the game. Your last high-score was " + PlayerPrefs.GetInt("endlessHiScore") + ".";
    private string optionsFlavour = "User options.";
    private string creditsFlavour = "Credits.";
    //private string exitFlavour = "Quit the game.";
    private string[] menuFlavourArray;

    //Options
    private string effectsVolFlavour = "Overall volume of the game.";
    private string musicVolFlavour = "Volume of the background music.";
    private string mouseSensFlavour = "Sensitivity of the mouse movement.";
    private string[] optionsFlavourArray;

    //General Variables
    private GameObject[] holderArray;
    private GameObject[] currentArray;
    private string[] currentFlavourArray;
    private Image[] currentImageArray;
    private GameObject selectedElement;
    private GameObject loadingScreen;
    private GameObject howToHolder;
    private GameObject creditsHolder;
    private GameObject titleHolder;
    private GameObject introHolder;
    private Vector3 uiSelectionPos;
    private float uiOffset = 90f;
    private float uiCloseOffset = 75f;
    private int selectedElemNum = 1;
    private int prevElemNum;
    private Color32 selectedColor = new Color32(255, 255, 225, 225);
    private Color32 fadedColor = new Color32(100, 100, 100, 225);
    private bool hasMoved = false;
    private AudioSource menuClick;
    private AudioSource menuConfirm;
    private AudioSource menuBack;

    public MainMenu() : base("Menu")
    {
        //Main Menu
        flavourTextHolder = GameObject.Find("FlavourText");
        flavourText = flavourTextHolder.GetComponent<Text>();
        backPromptUI = GameObject.Find("Back Prompt");
        howToUI = GameObject.Find("How to Play");
        playUI = GameObject.Find("Play");
        optionsUI = GameObject.Find("Options");
        creditsUI = GameObject.Find("Credits");
        //exitUI = GameObject.Find("Exit");
        menuHolder = GameObject.Find("Menu Holder");
        menuArray = new GameObject[] { howToUI, playUI, optionsUI, creditsUI };
        menuImageArray = new Image[] { howToUI.GetComponent<Image>(), playUI.GetComponent<Image>(), optionsUI.GetComponent<Image>(), creditsUI.GetComponent<Image>() };
        menuFlavourArray = new string[] { howToFlavour, playFlavour, optionsFlavour, creditsFlavour };

        //Options
        effectVolumeUI = GameObject.Find("Effects Volume");
        musicVolumeUI = GameObject.Find("Music Volume");
        mouseSensUI = GameObject.Find("Mouse Sensitivity");
        optionsHolder = GameObject.Find("Options Holder");
        optionsArray = new GameObject[] { effectVolumeUI, musicVolumeUI, mouseSensUI};
        optionsImageArray = new Image[] { effectVolumeUI.GetComponent<Image>(), musicVolumeUI.GetComponent<Image>(), mouseSensUI.GetComponent<Image>() };
        optionsFlavourArray = new string[] { effectsVolFlavour, musicVolFlavour, mouseSensFlavour };

        //Music & Effects Volume
        effectVolText = GameObject.Find("EffectsVolValue").GetComponent<Text>();
        musicVolText = GameObject.Find("MusicVolValue").GetComponent<Text>();
        mouseSensText = GameObject.Find("MouseSensValue").GetComponent<Text>();

        //General Variables
        menuClick = GameObject.Find("MenuMove").GetComponent<AudioSource>();
        menuConfirm = GameObject.Find("MenuConfirm").GetComponent<AudioSource>();
        menuBack = GameObject.Find("MenuBack").GetComponent<AudioSource>();
        howToHolder = GameObject.Find("HowToHolder");
        creditsHolder = GameObject.Find("CreditsHolder");
        titleHolder = GameObject.Find("Title");
        introHolder = GameObject.Find("IntroHolder");
        holderArray = new GameObject[] { menuHolder, optionsHolder, howToHolder, creditsHolder, introHolder };
        selectedElement = playUI;
        currentFlavourArray = menuFlavourArray;
        loadingScreen = GameObject.Find("Loading Screen");
        loadingScreen.SetActive(false);
    }

    public override void LevelStart()
    {
        if (!PlayerPrefs.HasKey("effectsVolume"))
            PlayerPrefs.SetInt("effectsVolume", 5);
        if (!PlayerPrefs.HasKey("musicVolume"))
            PlayerPrefs.SetInt("musicVolume", 5);
        if (!PlayerPrefs.HasKey("mouseSens"))
            PlayerPrefs.SetFloat("mouseSens", 0.5f);
        uiSelectionPos = howToUI.transform.position;
        SetMenuSelection(introHolder, null, null, null);
    }

    public override void LevelUpdate()
    {
        MoveUpdate();
        InputUpdate();
    }

    //Method for animating the main menu elements to make it feel smoother
    //Uses currentArray to assign and reference other arrays which hold menus
    public void MoveUpdate()
    {
        if (currentArray == null || currentImageArray == null)
            return;
        //Looping through the array to determine the selected menu option, and fade out unselected options
        for (int i = 0; i < currentArray.Length; i++)
        {
            int currentToSelectedDiff = Mathf.Abs(i - selectedElemNum);
            Vector3 tempPos = currentArray[i].transform.position;
            if (currentToSelectedDiff == 0)
            {
                selectedElement = currentArray[i];
                currentArray[i].transform.localScale = new Vector3(1, 1, 1);
                flavourText.text = currentFlavourArray[i];
                currentImageArray[i].color = selectedColor;
                if (!hasMoved)
                {
                    int moveSegments = 10;
                    for (int j = 0; j < moveSegments; j++)
                    {
                        float uiNextPosDiff = Mathf.Abs(currentArray[i].transform.position.y - uiSelectionPos.y);
                        if (currentArray[i].transform.position.y > uiSelectionPos.y)
                        {
                            tempPos.y -= uiNextPosDiff / moveSegments;
                            currentArray[i].transform.position = tempPos;
                        }

                        else if (currentArray[i].transform.position.y < uiSelectionPos.y)
                        {
                            tempPos.y += uiNextPosDiff / moveSegments;
                            currentArray[i].transform.position = tempPos;
                        }
                        if (j >= moveSegments)
                        {
                            hasMoved = true;
                        }
                    }
                }
            }
            else if (i > selectedElemNum)
            {
                tempPos = uiSelectionPos;
                if (Mathf.Abs(i - selectedElemNum) > 1)
                    tempPos.y -= (uiCloseOffset * currentToSelectedDiff);
                else
                    tempPos.y -= uiOffset;
                currentArray[i].transform.position = tempPos;
                currentArray[i].transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                currentImageArray[i].color = fadedColor;
            }

            else if (i < selectedElemNum)
            {
                tempPos = uiSelectionPos;
                if (Mathf.Abs(i - selectedElemNum) > 1)
                    tempPos.y += (uiCloseOffset * currentToSelectedDiff);
                else
                    tempPos.y += uiOffset;
                currentArray[i].transform.position = tempPos;
                currentArray[i].transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                currentImageArray[i].color = fadedColor;
            }
        }
    }

    //Input method for navigating the menu
    public void InputUpdate()
    {
        if (currentArray == optionsArray || howToHolder.activeSelf || creditsHolder.activeSelf)
        {
            backPromptUI.SetActive(true);            
            if (howToHolder.activeSelf || creditsHolder.activeSelf)
            {
                titleHolder.SetActive(false);
                flavourTextHolder.SetActive(false);
            }
        }
        else
        {
            backPromptUI.SetActive(false);
            flavourTextHolder.SetActive(true);
            titleHolder.SetActive(true);
        }
        if (Input.GetKeyDown("up") && selectedElemNum > 0)
        {
            menuClick.Play();
            selectedElemNum--;
            hasMoved = false;
        }

        if (Input.GetKeyDown("down") && selectedElemNum < currentArray.Length - 1)
        {
            menuClick.Play();
            selectedElemNum++;
            hasMoved = false;
        }

        //Options
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
                if( mouseSens >= 1.0f)
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

        if (Input.GetButtonDown("Confirm"))
        {
            prevElemNum = selectedElemNum;

            //Main Menu
            if (currentArray == menuArray)
            {
                if (selectedElement == howToUI)
                {
                    menuConfirm.Play();
                    SetMenuSelection(howToHolder, null, null, null);
                }
                if (selectedElement == playUI)
                {
                    menuConfirm.Play();
                    loadingScreen.SetActive(true);
                    SceneManager.LoadScene(sceneName: "Endless");
                }
                if (selectedElement == optionsUI)
                {
                    menuConfirm.Play();
                    effectVolume = PlayerPrefs.GetInt("effectsVolume");
                    musicVolume = PlayerPrefs.GetInt("musicVolume");
                    mouseSens = PlayerPrefs.GetFloat("mouseSens");
                    effectVolText.text = effectVolume.ToString();
                    musicVolText.text = musicVolume.ToString();
                    mouseSensText.text = mouseSens.ToString();
                    SetMenuSelection(optionsHolder, optionsArray, optionsImageArray, optionsFlavourArray);
                }
                if (selectedElement == creditsUI)
                {
                    menuConfirm.Play();
                    SetMenuSelection(creditsHolder, null, null, null);
                }
            }
        }

        if (Input.GetButtonDown("Back") && !menuHolder.activeSelf && !introHolder.activeSelf)
        {
            menuBack.Play();
            if (currentArray == optionsArray)
                PlayerPrefs.Save();
            SetMenuSelection(menuHolder, menuArray, menuImageArray, menuFlavourArray);
            selectedElemNum = prevElemNum;
        }

        if (Input.GetButtonDown("Confirm") && introHolder.activeSelf)
        {
            menuConfirm.Play();
            SetMenuSelection(menuHolder, menuArray, menuImageArray, menuFlavourArray);
            selectedElemNum = 0;
        }
    }

    //Method for setting a specific menu, used for hopping from one menu to another via a menu option
    public void SetMenuSelection(GameObject menuToEnable, GameObject[] arrayToSwitch, Image[] imageArrayToSwitch, string[] flavourArrayToSwitch)
    {
        for (int i = 0; i < holderArray.Length; i++)
        {
            holderArray[i].SetActive(false);
        }

        menuToEnable.SetActive(true);
        currentArray = arrayToSwitch;
        currentImageArray = imageArrayToSwitch;
        currentFlavourArray = flavourArrayToSwitch;
        selectedElemNum = 0;
    }
}
