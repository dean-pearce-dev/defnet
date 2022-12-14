using UnityEngine;

public class WallSpinner : MonoBehaviour
{
    //Simple class for spinning the semi-circle obstacles
    private float speed = 0.6f;
    private GameObject spinObject;

    public WallSpinner(GameObject objectToSpin)
    {
        spinObject = objectToSpin;
    }

    public void SpinCircle()
    {
        spinObject.transform.Rotate(0, 0, speed);
    }
}
