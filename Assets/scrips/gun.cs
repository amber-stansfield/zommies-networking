using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class gun : MonoBehaviour
{

    public GameObject owner;

    private Transform gunPos;
    private Transform gunAimPos;


    public Transform leftHandIkTargetFollower;
    public Transform rightHandIkTargetFollower;

    public Transform leftHandIkTarget;
    public Transform rightHandIkTarget;

    private Animator animator;

    private float adsPos;

    public bool isAnimating;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = transform.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (owner == null)
        {
            return;
        }
        //leftHandIkTargetFollower.transform.position = leftHandIkTarget.transform.position;

        //rightHandIkTargetFollower.transform.position = rightHandIkTarget.transform.position;


        if (Input.GetKeyDown(KeyCode.I) && adsPos < 0.03f)
        {
            //animator.ResetTrigger("inspect");
            //animator.enabled = true;

            animator.SetTrigger("inspect");
        }

    }


    private void LateUpdate()
    {

        if (owner == null)
        {
            return;
        }
        if (animator.GetNextAnimatorStateInfo(0).IsTag("1"))
        {

            isAnimating = true;
            //leftHandIkTargetFollower.transform.position = leftHandIkTarget.transform.position;

            //rightHandIkTargetFollower.transform.position = rightHandIkTarget.transform.position;

            Debug.Log("animating");
            return;
        }
        else
        {
            isAnimating = false;
            //animator.enabled = false;
        }


        transform.position = Vector3.Lerp(gunPos.position, gunAimPos.position, adsPos);
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Debug.Log($"woop");
            adsPos = Mathf.Lerp(adsPos, 1, Time.deltaTime * 9f);
            //owner.transform.GetComponent<mainGuy>().isAiming = true;


            transform.position = Vector3.Lerp(transform.position, gunAimPos.position, 0.4f);

        }
        else
        {
            adsPos = Mathf.Lerp(adsPos, 0, Time.deltaTime * 12);
            //owner.transform.GetComponent<mainGuy>().isAiming = false;

            transform.position = Vector3.Lerp(transform.position, gunPos.position, 0.1f);
        }

        if (owner != null)
        {
            transform.eulerAngles = gunPos.eulerAngles;
            //transform.eulerAngles = new Vector3(owner.transform.eulerAngles.x,owner.transform.eulerAngles.y -90,-ownerCam.transform.eulerAngles.x );
        }

    }

    public void interaction(GameObject ownerI, Transform gunPosI,Transform gunAimPosI)
    {
        owner = ownerI;
        gunPos = gunPosI;
        gunAimPos = gunAimPosI;
    }


}
