using UnityEngine;
using UnityEngine.UI;

public class EndlessLevel : Level
{
    private UIHandler uiHandler;
    private Grapple grappleObj;
    private Player player;
    private Rigidbody playerRb;
    private GameObject playerObj, backwall, endSegment;
    private GameObject[] segmentArray;
    private Vector3[] segmentDefaultPosArray;
    private Vector3 backwallDefaultPos, endSegmentDefaultPos, firstPointThreshold;
    private GameObject[] spinObjects;
    private WallSpinner[] wallSpinnerArray;
    private Text scoreHolder, finalScoreHolder, highScoreHolder;
    private string scoreString, finalScoreString, highScoreString;
    private float yLowerLimit = 1f;
    private float segmentDistance = 90f;
    private float segmentDefaultZPos;
    private int currentScore = 0;
    private int highScore;
    private bool firstSegmentPassed = false;
    private bool pauseState = false;
    private bool firstPointScored = false;
    private AudioSource failSound;
    private AudioSource checkpointSound;

    public EndlessLevel(Grapple grappleObject, Player playerObject, UIHandler uiHandlerObj) : base("Endless")
    {
        uiHandler = uiHandlerObj;
        player = playerObject;
        playerObj = player.GetPlayerObject();
        playerRb = playerObj.GetComponent<Rigidbody>();
        grappleObj = grappleObject;
        backwall = GameObject.Find("Backwall");
        endSegment = GameObject.Find("EndSegment");
        segmentArray = new GameObject[] { GameObject.Find("Segment1"), GameObject.Find("Segment2"), GameObject.Find("Segment3"), GameObject.Find("Segment4") };
        segmentDefaultPosArray = new Vector3[] {segmentArray[0].transform.position, segmentArray[1].transform.position, segmentArray[2].transform.position, segmentArray[3].transform.position };
        scoreHolder = GameObject.Find("Score Counter").GetComponent<Text>();
        finalScoreHolder = GameObject.Find("YouScored").GetComponent<Text>();
        highScoreHolder = GameObject.Find("HighScore").GetComponent<Text>();
        failSound = GameObject.Find("FailSound").GetComponent<AudioSource>();
        checkpointSound = GameObject.Find("Checkpoint").GetComponent<AudioSource>();

        uiHandler.Unpause();
    }

    public override void LevelStart()
    {
        if (PlayerPrefs.HasKey("endlessHiScore"))
            highScore = PlayerPrefs.GetInt("endlessHiScore");
        else if (!PlayerPrefs.HasKey("endlessHiScore"))
            PlayerPrefs.SetInt("endlessHiScore", 0);
        startPos = GameObject.Find("StartPos").transform.position;
        firstPointThreshold = GameObject.Find("FirstPointThreshold").transform.position;
        segmentDefaultZPos = segmentArray[0].transform.position.z;
        backwallDefaultPos = backwall.transform.position;
        endSegmentDefaultPos = endSegment.transform.position;
        backwall.SetActive(false);
    }

    public override void LevelUpdate()
    {
        pauseState = uiHandler.GetPauseState();
        CheckForFail();
        UpdateTunnelSegments();
        FirstPointCheck();
        DisplayScore();
    }

    private void DisplayScore()
    {
        scoreString = "Score:" + currentScore;
        scoreHolder.text = scoreString;
    }

    //Check if the player has passed the first obstacle, done this way in order to delay the movement of the tunnel segments until after the second checkpoint
    private void FirstPointCheck()
    {
        if (!firstPointScored && playerObj.transform.position.z > firstPointThreshold.z)
        {
            checkpointSound.Play();
            currentScore++;
            firstPointScored = true;
        }
    }

    //Checking if the player has fallen below the y limit for failing, and if so, displays the score and gives the player a prompt to restart
    //Uses PlayerPrefs to load and save high score
    private void CheckForFail()
    {
        if (playerObj.transform.position.y < yLowerLimit && !uiHandler.GetPauseState())
        {
            failSound.Play();
            if (currentScore > PlayerPrefs.GetInt("endlessHiScore"))
                PlayerPrefs.SetInt("endlessHiScore", currentScore);
            finalScoreString = "You Scored " + currentScore + "!";
            highScoreString = "High Score: " + PlayerPrefs.GetInt("endlessHiScore");
            finalScoreHolder.text = finalScoreString;
            highScoreHolder.text = highScoreString;
            PlayerPrefs.Save();
            uiHandler.EndlessConfirmBox();
        }
    }

    //Method for resetting the tunnel segments to their original position
    public override void ResetLevel()
    {
        for (int i = 0; i < segmentArray.Length; i++)
        {
            segmentArray[i].transform.position = segmentDefaultPosArray[i];
        }
        playerObj.transform.position = startPos;
        playerRb.velocity = Vector3.zero;
        player.SafePlayerReset();
        currentScore = 0;
        firstPointScored = false;
        firstSegmentPassed = false;
        backwall.transform.position = backwallDefaultPos;
        endSegment.transform.position = endSegmentDefaultPos;
        backwall.SetActive(false);
    }

    //Method for moving the tunnel segments, waits for player to pass a threshold, then moves the furthest back segment to the furthest forward spot
    //Increases score each time player passes an obstacle
    private void UpdateTunnelSegments()
    {
        for (int i = 0; i < segmentArray.Length; i++)
        {
            Vector3 nextPos = segmentArray[i].transform.position;
            Vector3 nextBackwallPos = backwall.transform.position;
            Vector3 nextEndSegmentPos = endSegment.transform.position;
            nextPos.z += segmentDistance * 4;
            nextBackwallPos.z += segmentDistance;
            nextEndSegmentPos.z += segmentDistance;
            if (playerObj.transform.position.z >= segmentArray[i].transform.position.z - 10f)
            {
                checkpointSound.Play();
                if (!firstSegmentPassed)
                {                    
                    firstSegmentPassed = true;
                    backwall.SetActive(true);
                }
                else if (firstSegmentPassed)
                {
                    backwall.transform.position = nextBackwallPos;
                    endSegment.transform.position = nextEndSegmentPos;
                }
                segmentArray[i].transform.position = nextPos;
                segmentArray[i].transform.GetChild(0).GetChild(0).GetComponent<WallSpinner>().RandomiseSpeed();
                currentScore++;
            }
        }
    }
}
