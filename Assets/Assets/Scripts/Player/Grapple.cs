using UnityEngine;

//https://www.youtube.com/watch?v=Xgh4v1w5DxU
//Code adapted from linked youtube video

public class Grapple : MonoBehaviour
{
    //Grapple information variables
    private Vector3 leftGrapplePoint, rightGrapplePoint, grapplePoint;
    private LayerMask grappleLayer, grappleStatic, grappleObject, grappleMoving;
    private GameObject leftGrappleMesh, rightGrappleMesh;
    private Rigidbody rbGrapple;
    private string leftGrappleType, rightGrappleType, grappleType;
    private bool isGrappling = false;
    private bool isLeftGrappling = false;
    private bool isRightGrappling = false;
    private float maxDistance = 25f;

    //Player & Component variables
    private GameObject camTarget, outerFrame;
    private Rigidbody playerRb;
    private SpringJoint leftJoint, rightJoint;
    private Transform leftGunTip, rightGunTip, mainCam, playerTransform;
    private LineRenderer leftLr, rightLr;
    private AudioSource grappleSound;

    //SpringJoint Variables
    private float initialDistance;
    private float jointSpring = 20f;
    private float jointDamper = 10f;
    private float jointMassScale = 2f;

    void Start()
    {
        //Getting references to relevant game components
        grappleLayer = LayerMask.GetMask("Ground", "GrappleMoveable", "GrappleMoving");
        grappleStatic = LayerMask.GetMask("Ground");
        grappleObject = LayerMask.GetMask("GrappleMoveable");
        grappleMoving = LayerMask.GetMask("GrappleMoving");
        leftGrappleMesh = GameObject.Find("LeftGrapple");
        rightGrappleMesh = GameObject.Find("RightGrapple");
        camTarget = GameObject.Find("camTarget");
        outerFrame = GameObject.Find("OuterFrame");
        leftGunTip = GameObject.Find("LeftGrapplePoint").transform;
        rightGunTip = GameObject.Find("RightGrapplePoint").transform;
        mainCam = GameObject.Find("Main Camera").transform;
        playerRb = GameObject.Find("Player").GetComponent<Rigidbody>();
        playerTransform = GameObject.Find("Player").transform;
        leftLr = GameObject.Find("LeftGrapple").GetComponent<LineRenderer>();
        rightLr = GameObject.Find("RightGrapple").GetComponent<LineRenderer>();
        grappleSound = GameObject.Find("GrappleSound").GetComponent<AudioSource>();
    }

    //Main Grapple Logic
    public void GrappleUpdate()
    {
        GrappleInput();
        GrappleCheck();
    }

    public void GrappleLateUpdate()
    {
        DrawRope();
    }

    //Method for grappling, uses raycast to check for hits in range, and adds a spring joint if a hit is returned. Also adds line renderer positions to act as a visible rope
    private void StartGrapple(string grappleSide)
    {
        RaycastHit hit;
        SpringJoint tempJoint;
        //Getting the distance between the camera, and the target above the player that it's aimed at. If a raycast hits something inbetween this space,
        //returns out of the method to prevent the player grappling objects behind them.
        float camToPlayerDist = Vector3.Distance(mainCam.position, camTarget.transform.position);
        if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, camToPlayerDist, grappleLayer))
            return;

        //Checking for hit on grappleable surfaces using a layer mask
        if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, grappleLayer))
        {
            grappleSound.Play();
            //Setting grappling bools, setting the hit point and surface type, and adding and setting the springjoint to act as the grapple rope
            tempJoint = playerTransform.gameObject.AddComponent<SpringJoint>();
            tempJoint.autoConfigureConnectedAnchor = false;
            tempJoint.enableCollision = true;
            float distanceFromPoint;

            //Conditional for grappling to either an object or a surface. Objects can be dragged and moved around by the player. Currently no moveable objects are implemented
            //so this first condition is unused
            if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, grappleObject) || Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, grappleMoving))
            {
                grappleType = "Object";
                rbGrapple = hit.rigidbody;
                tempJoint.connectedAnchor = new Vector3(0, 0, 0);
                tempJoint.connectedBody = rbGrapple;
                distanceFromPoint = Vector3.Distance(playerTransform.position, rbGrapple.position);
                if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, grappleMoving))
                {
                    tempJoint.maxDistance = distanceFromPoint;
                    tempJoint.minDistance = 1f;
                }
            }
            else if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, grappleStatic))
            {
                grappleType = "Surface";
                grapplePoint = hit.point;
                tempJoint.connectedAnchor = grapplePoint;
                distanceFromPoint = Vector3.Distance(playerTransform.position, grapplePoint);
                tempJoint.maxDistance = distanceFromPoint * 0.7f;
                tempJoint.minDistance = distanceFromPoint * 0.2f;
            }

            //Setting joint variables. These affect the physics and strength of the grapple
            tempJoint.spring = jointSpring;
            tempJoint.damper = jointDamper;
            tempJoint.massScale = jointMassScale;

            //Setting line renderer positions, and grapple variables to the correct side
            if (grappleSide == "Left")
            {
                leftLr.positionCount = 2;
                leftJoint = tempJoint;
                leftGrappleType = grappleType;
                if (grappleType == "Surface")
                    leftGrapplePoint = grapplePoint;
                else
                    leftGrapplePoint = rbGrapple.position;
            }
            else if (grappleSide == "Right")
            {
                rightLr.positionCount = 2;
                rightJoint = tempJoint;
                rightGrappleType = grappleType;
                if (grappleType == "Surface")
                    rightGrapplePoint = grapplePoint;
                else
                    rightGrapplePoint = rbGrapple.position;
            }
        }
    }

    //If Left Mouse is clicked, starts the grapple, and stops it on release
    //Right Mouse does the same for the right side grapple
    private void GrappleInput()
    {
        if (Input.GetButtonDown("Left Grapple"))
        {
            StartGrapple("Left");
        }
        else if (Input.GetButtonUp("Left Grapple"))
        {
            StopGrapple("Left");
        }
        if (Input.GetButtonDown("Right Grapple"))
        {
            StartGrapple("Right");
        }
        else if (Input.GetButtonUp("Right Grapple"))
        {
            StopGrapple("Right");
        }
    }

    //Method for drawing the rope on grapple
    private void DrawRope()
    {
        //If no joint exists, returns out of the method
        if (!leftJoint && !rightJoint)
            return;
        //Sets the line renderer positions for the grapple
        if (leftJoint)
        {
            leftLr.SetPosition(0, leftGunTip.position);
            leftLr.SetPosition(1, leftGrapplePoint);
        }
        if (rightJoint)
        {
            rightLr.SetPosition(0, rightGunTip.position);
            rightLr.SetPosition(1, rightGrapplePoint);
        }
    }

    //Method for checking if the player isGrappling, and making the grapple guns look towards their grapple target
    public void GrappleCheck()
    {
        if (leftJoint || rightJoint)
        {
            if (leftJoint)
            {
                isLeftGrappling = true;
                if (leftGrappleType == "Surface")
                    leftGrappleMesh.transform.LookAt(leftGrapplePoint);
                else
                {
                    leftGrapplePoint = leftJoint.connectedBody.position;
                    leftGrappleMesh.transform.LookAt(leftGrapplePoint);
                }
            }
            if (rightJoint)
            {
                isRightGrappling = true;
                if (rightGrappleType == "Surface")
                    rightGrappleMesh.transform.LookAt(rightGrapplePoint);
                else
                {
                    rightGrapplePoint = rightJoint.connectedBody.position;
                    rightGrappleMesh.transform.LookAt(rightGrapplePoint);
                }
            }
            isGrappling = true;
        }

        //If either joint returns null, sets the neccessary values for not grappling
        if (!leftJoint || !rightJoint)
        {
            if (!leftJoint)
            {
                isLeftGrappling = false;
                leftLr.positionCount = 0;
                leftGrappleMesh.transform.rotation = outerFrame.transform.rotation;
            }
            if (!rightJoint)
            {
                isRightGrappling = false;
                rightLr.positionCount = 0;
                rightGrappleMesh.transform.rotation = outerFrame.transform.rotation;
            }
            if (!leftJoint && !rightJoint)
            isGrappling = false;
        }
    }

    //Method for getting the joint, side is chosen by a string parameter
    public Rigidbody CheckJointRb(string grappleSide)
    {
        if (grappleSide == "Left")
            return leftJoint.connectedBody;
        else if (grappleSide == "Right")
            return rightJoint.connectedBody;
        else
            return null;
    }

    //Method for stopping the grapple
    public void StopGrapple(string grappleSide)
    {
        if (grappleSide == "Left")
        Destroy(leftJoint);
        if (grappleSide == "Right")
        Destroy(rightJoint);
        else if (grappleSide == "Both")
        {
            Destroy(leftJoint);
            Destroy(rightJoint);
        }
    }

    //Getters and Setters
    public bool IsGrappling()
    {
        return isGrappling;
    }

    public bool IsLeftGrappling()
    {
        return isLeftGrappling;
    }

    public bool IsRightGrappling()
    {
        return isRightGrappling;
    }
}
