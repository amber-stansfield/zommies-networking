using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class gun : NetworkBehaviour
{

    public GameObject owner;

    private Transform gunPos;
    private Transform gunAimPos;


    public Transform leftHandIkTargetFollower;
    public Transform rightHandIkTargetFollower;

    public Transform leftHandIkTarget;
    public Transform rightHandIkTarget;

    private Animator animator;

    //private Transform parent;

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

        this.transform.parent.position = gunPos.transform.position;

        this.transform.parent.rotation = gunPos.transform.rotation;

        if (Input.GetKeyDown(KeyCode.I) && adsPos < 0.03f && (owner.transform.parent.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>().OwnerClientId))
        {
            //animator.ResetTrigger("inspect");
            //animator.enabled = true;
            //animator.SetTrigger("inspect");
            startAnimationServerRpc("inspect");
            
        }

    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void startAnimationServerRpc(string animation)
    {
        animator.SetTrigger(animation);
        startAnimationClientRpc(animation);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    void startAnimationClientRpc(string animation)
    {
        //if (IsOwner) { return; }
        animator.SetTrigger(animation);
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
        //lerpServerRpc(gunPos.position, gunAimPos.position, adsPos);
        if (owner.transform.parent.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>().OwnerClientId)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                adsPos = Mathf.Lerp(adsPos, 1, Time.deltaTime * 9f);

                transform.position = Vector3.Lerp(transform.position, gunAimPos.position, 0.4f);

                //owner.transform.GetComponent<mainGuy>().isAiming = true;


                //lerpServerRpc(transform.position, gunAimPos.position, 0.1f);
            }
            else
            {
                adsPos = Mathf.Lerp(adsPos, 0, Time.deltaTime * 12);
                //owner.transform.GetComponent<mainGuy>().isAiming = false;

                transform.position = Vector3.Lerp(transform.position, gunPos.position, 0.1f);
                //lerpServerRpc(transform.position, gunPos.position, 0.1f);
            }
            if (adsPos < 0.01f) { adsPos = 0; }
            else if (adsPos > 0.99f) { adsPos = 1; }

            aimServerRpc(adsPos);
        }

        if (owner != null)
        {
            transform.eulerAngles = gunPos.eulerAngles;
            //transform.eulerAngles = new Vector3(owner.transform.eulerAngles.x,owner.transform.eulerAngles.y -90,-ownerCam.transform.eulerAngles.x );
        }

    }

    [Rpc(SendTo.Server,InvokePermission = RpcInvokePermission.Everyone)]
    void aimServerRpc(float AdsPos)
    {
        adsPos = AdsPos;
        transform.position = Vector3.Lerp(transform.position, gunAimPos.position, 0.4f);
        
        aimClientRpc(AdsPos);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void aimClientRpc(float AdsPos)
    {
        if (owner.transform.parent.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>().OwnerClientId) { return; }
        adsPos = AdsPos;
        transform.position = Vector3.Lerp(transform.position, gunAimPos.position, 0.4f);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void lerpServerRpc(Vector3 current,Vector3 target, float amount)
    {
        transform.position = Vector3.Lerp(current, target, amount);
        lerpClientRpc(current, target, amount);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void lerpClientRpc(Vector3 current, Vector3 target, float amount)
    {
        if (IsHost) { return; }
        transform.position = Vector3.Lerp(current, target, amount);
    }

    public void interaction(GameObject ownerI, Transform gunPosI, Transform gunAimPosI, Transform ownerCameraPos, GameObject leftArmRig, GameObject rightArmRig, GameObject armParent)
    {
        owner = ownerI;
        gunPos = gunPosI;
        gunAimPos = gunAimPosI;

        this.transform.position = ownerCameraPos.GetComponent<camera>().gunPos.transform.position;

        this.transform.rotation = ownerCameraPos.GetComponent<camera>().gunPos.transform.rotation;

        this.transform.GetComponentInParent<Rigidbody>().isKinematic = true;
        this.transform.GetComponentInParent<BoxCollider>().isTrigger = true;
        leftArmRig.GetComponent<ChainIKConstraint>().data.target = this.transform.parent.transform.GetChild(1).transform;
        rightArmRig.GetComponent<ChainIKConstraint>().data.target = this.transform.parent.transform.GetChild(2).transform;

        armParent.gameObject.SetActive(false);
        armParent.gameObject.SetActive(true);

    }


    public void drop()
    {
        owner = null;
        this.transform.GetComponentInParent<Rigidbody>().isKinematic = false;
        this.transform.GetComponentInParent<BoxCollider>().isTrigger = false;
    }

}
