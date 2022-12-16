using UnityEngine;

public class WallSpinner : MonoBehaviour
{
    //Simple class for spinning the semi-circle obstacles
    private float speed = 60.0f;
    private float maxSpeed = 80.0f;
    private float minSpeed = 50.0f;

    void Start()
    {
        RandomiseSpeed();
    }

    void Update()
    {
        SpinCircle();
    }

    public void SpinCircle()
    {
        gameObject.transform.Rotate(0, 0, speed * Time.deltaTime);
    }

    public void RandomiseSpeed()
    {
        speed = Random.Range(minSpeed, maxSpeed);
    }
}
