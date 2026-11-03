using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    public Vector2 velocity;
    Vector2 frameVelocity;
    

    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Lock the mouse cursor to the game screen.
       // Cursor.lockState = CursorLockMode.Locked;
    }

public void Look(Vector2 mouseDelta)
    {
    if(Time.timeScale>0)
       {
        mouseDelta *= 0.5f;
        // Get smooth velocity.
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
       transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
       character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
       }
    }

 public void odrzut(float odrzut_poziom,float odrzut_pion1,float odrzut_pion2)
 {
  velocity.x+=Random.Range(-odrzut_poziom,odrzut_poziom);
  velocity.y+=Random.Range(odrzut_pion1,odrzut_pion2);
 }

}
