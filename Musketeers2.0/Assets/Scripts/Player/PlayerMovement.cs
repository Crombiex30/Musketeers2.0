using UnityEngine;

public class PlayerMovement:MonoBehaviour
{
    public Rigidbody body;              // This is the call to the rigidbody placed into a variable
    public int speed;                   //This speed is the movement speed

    private void Start()
    {
        body = GetComponent<Rigidbody>();       
    }

    private void Update()
    {
        
        float horizontalInput = Input.GetAxis("Horizontal")* -1;    //This code helps with side to side movement
        float verticalInput = Input.GetAxis("Vertical")* -1;        //This code helps with forward and backwards movement

        body.linearVelocity = new Vector3(horizontalInput*speed,body.linearVelocity.y, verticalInput*speed);

    }
}
