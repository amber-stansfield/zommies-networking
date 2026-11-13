using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

public class mainGuy : MonoBehaviour
{
    public GameObject cameraObj;

    public bool isAiming;

    public float runSpeed;
    public float speed;
    public float aimingSpeed;
    private float currentSpeed;
    private GameObject heldItem;

    [SerializeField] GameObject leftArmRig;
    [SerializeField] GameObject rightArmRig;

    [SerializeField] GameObject armParent;

    [SerializeField] Transform defaultLeftArmTarget;
    [SerializeField] Transform defaultRightArmTarget;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E) && heldItem == null)
        {
            Debug.DrawRay(cameraObj.transform.position, cameraObj.transform.forward, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(cameraObj.transform.position, cameraObj.transform.forward, out hit, 500))
            {
                if (hit.transform.CompareTag("pickupable"))
                {
                    heldItem = hit.transform.gameObject;

                    hit.transform.GetChild(0).GetComponent<gun>().interaction(this.gameObject, cameraObj.GetComponent<camera>().gunPos.transform, cameraObj.GetComponent<camera>().gunAimPos.transform);

                    hit.transform.SetParent(cameraObj.transform);

                    hit.transform.position = cameraObj.transform.GetComponent<camera>().gunPos.transform.position;

                    hit.transform.rotation = cameraObj.transform.GetComponent<camera>().gunPos.transform.rotation;

                    hit.transform.GetComponent<Rigidbody>().isKinematic = true;

                    leftArmRig.GetComponent<ChainIKConstraint>().data.target = hit.transform.GetChild(1).transform;
                    rightArmRig.GetComponent<ChainIKConstraint>().data.target = hit.transform.GetChild(2).transform;

                    armParent.gameObject.SetActive(false);
                    armParent.gameObject.SetActive(true);

                }
            }
        }

        

        if (isAiming)
        {
            currentSpeed = aimingSpeed;
        }

        else if (!isAiming && Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }

        else
        {
            currentSpeed = speed;
        }

        transform.eulerAngles = new Vector3(transform.rotation.x, cameraObj.transform.eulerAngles.y, transform.rotation.z);

        Vector3 dir = new Vector3(0, 0, 0);
        dir.x = Input.GetAxis("Horizontal");
        dir.z = Input.GetAxis("Vertical");
        transform.Translate(dir * currentSpeed * Time.deltaTime);
    }
}
