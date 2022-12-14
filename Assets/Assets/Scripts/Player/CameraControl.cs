using UnityEngine;
using UnityEngine.UI;

//http://wiki.unity3d.com/index.php?title=MouseOrbitImproved&oldid=18976
//Modified to be used in OOP
public class CameraControl : MonoBehaviour
{
    //Grapple Variables
    private LayerMask grappleLayer;
    private LayerMask grappleableSurface;
    private LayerMask grappleMoveable;
    private GameObject grappleGun;
    private bool isGrappleable = false;
    private bool isGrappling = false;
    private bool isLeftGrappling = false;
    private bool isRightGrappling = false;
    private float maxGrappleDist = 25f;

    //Camera Variables
    private GameObject targetObj;
    private GameObject mainCamera;
    private GameObject moveTarget;
    private GameObject outerFrame;
    private Rigidbody cameraRb;
    private Transform target;

    //Camera default distance and move speed
    private float distance = 5.0f;
    private float xSpeed = 20.0f;
    private float ySpeed = 50.0f;

    //Camera min and max rotation angles
    private float yMinLimit = -75f;
    private float yMaxLimit = 80f;

    //Min and max distance between camera and player
    private float distanceMin = 0.5f;
    private float distanceMax = 15f;

    //fixedDistance = distance the camera will be if no object is in the way
    //clippingRate = used to affect camera move increments to avoid object clipping
    private float fixedDistance = 5.0f;
    private float clippingRate = 100f;
    private float autoZoomSpeed = 0.1f;

    //Crosshair Variables
    private Image imgComponent;
    private GameObject crosshair;
    private GameObject leftGrappleCrosshair;
    private GameObject rightGrappleCrosshair;
    private GameObject leftGrapplingCrosshair;
    private GameObject rightGrapplingCrosshair;
    private GameObject[] crosshairArray = new GameObject[4];

    float x = 0.0f;
    float y = 0.0f;

    public CameraControl()
    {
        grappleLayer = LayerMask.GetMask("Ground", "GrappleMoveable", "GrappleMoving");
        grappleableSurface = LayerMask.GetMask("Ground");
        grappleMoveable = LayerMask.GetMask("GrappleMoveable");
        targetObj = GameObject.Find("camTarget");
        mainCamera = GameObject.Find("Main Camera");
        moveTarget = GameObject.Find("MoveTarget");
        grappleGun = GameObject.Find("GrappleGun");
        outerFrame = GameObject.Find("OuterFrame");
        crosshair = GameObject.Find("Crosshair");
        leftGrappleCrosshair = GameObject.Find("LeftGrappleCrosshair");
        rightGrappleCrosshair = GameObject.Find("RightGrappleCrosshair");
        leftGrapplingCrosshair = GameObject.Find("LeftGrapplingCrosshair");
        rightGrapplingCrosshair = GameObject.Find("RightGrapplingCrosshair");

        Vector3 angles = mainCamera.transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        cameraRb = mainCamera.GetComponent<Rigidbody>();
        target = targetObj.transform;

        // Make the rigid body not change rotation
        if (cameraRb != null)
        {
            cameraRb.freezeRotation = true;
        }

        //Array for setting the crosshair depending on whether the player can grapple, and/or if they're already grappling
        crosshairArray[0] = leftGrapplingCrosshair;
        crosshairArray[1] = rightGrapplingCrosshair;
        crosshairArray[2] = leftGrappleCrosshair;
        crosshairArray[3] = rightGrappleCrosshair;
    }

    //Main update logic
    public void CameraUpdate(bool grapple, bool leftGrapple, bool rightGrapple)
    {
        //Setting the private grapple bools to the ones passed in by the paramaters
        isGrappling = grapple;
        isLeftGrappling = leftGrapple;
        isRightGrappling = rightGrapple;

        //Using GrappleableCheck to set the grapple bools, then CrosshairCheck in CameraLateUpdate to set the correct crosshair
        GrappleableCheck();
    }

    public void CameraLateUpdate()
    {
        CrosshairCheck();
        if (target)
        {
            //Making sure moveTarget matches the current position to accurately track the next move
            moveTarget.transform.position = mainCamera.transform.position;

            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast(target.position, mainCamera.transform.position, out hit))
            {
                //Dividing hit.distance by clippingRate to make the auto camera zoom smoother
                distance -= (hit.distance / clippingRate);
            }
            else if (!Physics.Linecast(target.position, mainCamera.transform.position, out hit))
            {
                //While loop to smoothly reset the camera back to the set distance
                //Uses moveTarget and temp variables to determine if the next move would cause a collision
                //If so, cancels out the move so that the camera doesn't bounce between detection and non-detection
                while (distance < fixedDistance)
                {
                    distance += autoZoomSpeed;
                    Vector3 tempNegDistance = new Vector3(0.0f, 0.0f, -distance);
                    Vector3 nextPos = rotation * tempNegDistance + target.position;
                    moveTarget.transform.position = nextPos;
                    if (Physics.Linecast(target.position, moveTarget.transform.position, out hit))
                        distance -= autoZoomSpeed;
                    break;
                }
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            //Setting the transform for the camera, and matching the player's outer frame rotation to it
            //also resetting moveTarget so it can be used on the next run
            mainCamera.transform.rotation = rotation;
            mainCamera.transform.position = position;
            moveTarget.transform.position = mainCamera.transform.position;
            outerFrame.transform.rotation = rotation;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    //Method for checking if a target is in range and grappleable so that CrosshairCheck can display the correct crosshair
    private void GrappleableCheck()
    {
        //Using a raycast to determine if a grappleable point is in range
        //If so another raycast is used to make sure it isn't in the space between the player and the camera
        //The same check is used in the Grapple class to prevent grappling behind the player, it is used in this class to determine which crosshair to display
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, maxGrappleDist, grappleLayer))
        {
            float camToPlayerDist = Vector3.Distance(mainCamera.transform.position, targetObj.transform.position);
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, camToPlayerDist, grappleLayer))
                isGrappleable = false;
            else
                isGrappleable = true;
        }
        else
            isGrappleable = false;
    }

    //Method which disables all crosshairs, then checks conditions to determine which crosshair to enable
    private void CrosshairCheck()
    {
        DisableCrosshairs();
        if (isLeftGrappling)
            leftGrapplingCrosshair.SetActive(true);
        if (isRightGrappling)
            rightGrapplingCrosshair.SetActive(true);
        if (isGrappleable)
        {
            if (!isLeftGrappling)
                leftGrappleCrosshair.SetActive(true);
            if (!isRightGrappling)
                rightGrappleCrosshair.SetActive(true);
        }
    }

    //Method for disabling all crosshairs at once
    private void DisableCrosshairs()
    {
        for (int i = 0; i < crosshairArray.Length; i++)
        {
            crosshairArray[i].SetActive(false);
        }
    }
}
