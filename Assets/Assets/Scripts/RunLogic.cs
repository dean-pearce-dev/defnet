using UnityEngine;
using UnityEngine.SceneManagement;

public class RunLogic : MonoBehaviour
{
    //Main logic script
    //This is the only script attached to a game object in the scene, and all code is controlled from here
    //Runs each object's Start/Update scripts and passes through multiple references in paramaters to give
    //control to the underlying objects
    private Player player;
    private CameraControl mainCam;
    private Grapple grapple;
    private UIHandler uiHandler;
    private LevelManager levelManager;

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Menu")
        {
            player = new Player();
            mainCam = new CameraControl();
            grapple = new Grapple();
            uiHandler = new UIHandler(player, grapple);
        }
        levelManager = new LevelManager(grapple, player, uiHandler);
        levelManager.LevelManagerStart();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Menu")
        {
            if (!uiHandler.GetPauseState())
            {
                mainCam.CameraUpdate(grapple.IsGrappling(), grapple.IsLeftGrappling(), grapple.IsRightGrappling());
                grapple.GrappleUpdate();
                player.PlayerUpdate();
            }
            levelManager.LevelManagerUpdate();
        }
        else
            levelManager.LevelManagerUpdate();
    }

    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name != "Menu")
            if (!uiHandler.GetPauseState())
                player.PlayerFixedUpdate(grapple.IsGrappling());
    }

    void LateUpdate()
    {
        if (SceneManager.GetActiveScene().name != "Menu")
        {
            if (!uiHandler.GetPauseState())
            {
                mainCam.CameraLateUpdate();
                grapple.GrappleLateUpdate();
            }
            uiHandler.UIUpdate();
        }
    }
}
