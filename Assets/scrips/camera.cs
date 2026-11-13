using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class camera : MonoBehaviour
{

    [SerializeField] float sens;

    [SerializeField] Transform playerObjTransform;

    [SerializeField] Transform followPos;

    public GameObject gunPos;
    public GameObject gunAimPos;

    private Vector3 camSmoothVelocity = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {

        playerObjTransform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X"), 0);

        transform.position = Vector3.SmoothDamp(transform.position,followPos.position, ref camSmoothVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(this.transform.eulerAngles.x, playerObjTransform.transform.eulerAngles.y, this.transform.rotation.z);
        transform.rotation *= Quaternion.Euler(-Input.GetAxis("Mouse Y"), 0, 0);



    }
}
