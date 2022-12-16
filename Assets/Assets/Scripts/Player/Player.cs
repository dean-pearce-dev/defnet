using UnityEngine;

public class Player
{
    //Game Objects
    private GameObject playerObj;
    private GameObject mainCamera;
    private GameObject camTarget;
    private GameObject grappleGun;
    private GameObject outerFrame;
    private Rigidbody rb;

    //Player Variables
    private Vector3 startPos;
    private float camTargetYOffset = 3.0f;
    private float speed;
    private float groundSpeed = 27.5f;
    private float airSpeed = 17.5f;
    private float groundAccel = 0f;
    private float groundAccelIncrement = 0.2f;
    private float maxGroundAccel = 3.7f;
    private float airAccel = 0f;
    private float airAccelIncrement = 0.015f;
    private float maxAirAccel = 1.3f;
    private float maxGroundVelocity = 30f;
    private float maxAirVelocity = 22.5f;
    private float maxDownforce = 9f;
    private float airDropRate = 1.8f;
    private float airDropRateIncrement = 0.03f;
    private float timeElapsed = 0f;
    private Vector3 currentVelocity;
    private bool isBraking = false;
    private bool isGrounded = false;
    private bool isGrappling = false;
    private bool isMoveInput = false;

    public Player()
    {
        playerObj = GameObject.Find("Player");
        mainCamera = GameObject.Find("Main Camera");
        camTarget = GameObject.Find("camTarget");
        grappleGun = GameObject.Find("GrappleGun");
        outerFrame = GameObject.Find("OuterFrame");
        rb = playerObj.GetComponent<Rigidbody>();
        startPos = GameObject.Find("StartPos").transform.position;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void PlayerUpdate()
    {
        outerFrame.transform.position = playerObj.transform.position;
        GroundCheck();
        SpeedBrake();
        camTarget.transform.position = new Vector3(playerObj.transform.position.x, playerObj.transform.position.y + camTargetYOffset, playerObj.transform.position.z);
    }

    public void PlayerFixedUpdate(bool grapple)
    {
        isGrappling = grapple;
        Move();
    }

    private void Move()
    {
        //https://forum.unity.com/threads/using-addforce-relative-to-camera.924086/
        //Getting normalized Vectors from the camera to make movement work relative to the camera direction
        Vector3 moveDir = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z).normalized;
        Vector3 sideMoveDir = new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z).normalized;

        //Getting the current velocity of the player
        currentVelocity = rb.velocity;

        //Multiplier variables for allowing the ball to gain momentum
        if (isMoveInput && currentVelocity.magnitude > 7.5f && currentVelocity.magnitude < maxGroundVelocity && groundAccel < maxGroundAccel)
            groundAccel += groundAccelIncrement;
        if (currentVelocity.magnitude < 7.5f && groundAccel > 0f || !isMoveInput && groundAccel > 0f)
            groundAccel -= groundAccelIncrement;

        //If the player falls under a certain speed, acceleration values are reset
        if (currentVelocity.magnitude < 5f)
        {
            airAccel = 0;
            groundAccel = 0;
        }

        //Check for if the player is in the air
        if (!isGrounded)
        {
            timeElapsed = 0;
            //airAccel drops when player is not inputting movement
            if (!isMoveInput)
                airAccel -= airAccelIncrement / 100;
            if (currentVelocity.magnitude < 2f)
                airAccel = 0f;
            //Halves the movement speed
            airSpeed = (airSpeed + airAccel) / 2;

            //If the player speed is below maxVelocity, increases air acceleration values
            if (currentVelocity.magnitude < maxAirVelocity)
            {
                rb.AddForce(currentVelocity * airAccel);
                rb.AddForce(Vector3.down * airDropRate);
                if (airAccel < maxAirAccel && isMoveInput)
                airAccel += airAccelIncrement;

                //Adds increasing downforce the longer the player is in the air
                if (airDropRate < maxDownforce && currentVelocity.magnitude > 7f)
                    airDropRate += airDropRateIncrement;
                //Increases force from player input whilst grappling in the air
                if (isGrappling)
                {
                        airSpeed = airSpeed * 1.2f;
                }
            }
            //Allows the ball to continue spinning after brake has been used
            if (!isBraking)
                rb.AddTorque(currentVelocity / 3f);
            speed = airSpeed;
            groundAccel = 0f;
        }

        //Resets acceleration variables once player touches the ground
        if (isGrounded)
        {
            if (timeElapsed < 1f)
                timeElapsed += Time.deltaTime;
            if (airAccel > 0f && timeElapsed >= 1f)
                airAccel = 0f;
            airDropRate = 2f;
            speed = groundSpeed;
        }

        //Input for moving the player ball,  WASD for direction, checking if the player is braking to disable forward and backward force, and reduce sideways force
        if (!isBraking || isBraking && !isGrounded)
        {
            if (Input.GetButton("Forward") || Input.GetButton("Left") || Input.GetButton("Backward") || Input.GetButton("Right"))
                isMoveInput = true;
            else
                isMoveInput = false;
            if (Input.GetButton("Forward"))
            {
                rb.AddForce(moveDir * (speed + groundAccel));
            }
            if (Input.GetButton("Backward"))
            {
                rb.AddForce(-moveDir * (speed + groundAccel));
            }
            if (Input.GetButton("Left"))
            {
                rb.AddForce(-sideMoveDir * speed);
            }
            if (Input.GetButton("Right"))
            {
                rb.AddForce(sideMoveDir * speed);
            }

            //Adds extra downforce while player holds brake in the air, reduced if player is grappling
            if (isBraking)
            {
                if (isGrappling)
                    rb.AddForce(Vector3.down * maxDownforce / 2);
                else
                    rb.AddForce(Vector3.down * maxDownforce * 2);
            }
        }

        //If player is braking and on the ground, only allows small movement left & right
        if (isBraking && isGrounded)
        {
            if (Input.GetButton("Left"))
            {
                rb.AddForce(-sideMoveDir * (speed / 2));
            }
            if (Input.GetButton("Right"))
            {
                rb.AddForce(sideMoveDir * (speed / 2));
            }
        }

        //Resetting speed variables
        groundSpeed = 25f;
        airSpeed = 15f;
    }

    //Seperate Method for brake because this needs to be in Update() where as the Move() method needs to be in FixedUpdate()
    public void SpeedBrake()
    {
        if (Input.GetButtonDown("Brake/Airbrake"))
        {               
            rb.freezeRotation = true;
            isBraking = true;
        }
        if (Input.GetButtonUp("Brake/Airbrake"))
        {
            rb.freezeRotation = false;
            isBraking = false;
        }
    }

    //Grounded check to see if player is airborne
    private void GroundCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerObj.transform.position, Vector3.down, out hit, 1.4f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    //Method for resetting variables from pause screen, makes sure player keeps control of the player after unpause
    public void SafePlayerReset()
    {
        isBraking = false;
        rb.freezeRotation = false;        
    }

    //Getters and Setters
    public float GetSpeed()
    {
        return speed;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public GameObject GetPlayerObject()
    {
        return playerObj;
    }

    public float GetCurrentSpeed()
    {
        return currentVelocity.magnitude;
    }

    public float GetGroundAccel()
    {
        return groundAccel;
    }

    public float GetAirAccel()
    {
        return airAccel;
    }

    public float GetDownforce()
    {
        return airDropRate;
    }
}
