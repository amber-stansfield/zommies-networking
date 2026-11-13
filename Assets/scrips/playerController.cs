using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class playerGamer : NetworkBehaviour
{

    Vector2 move;

    [SerializeField] float defaultMoveSpeed, maxSpeed, sprintMult, crouchMult, iFrames, regenDelay,
    defaultStamina, recoveryCoolDown, cameraBobSpeed, cameraBobAmplitude, health, timeSinceLastDamage;



    [SerializeField] KeyCode forward, backward, left, right, sprint, crouch, jump;
    [SerializeField] int startingHealth;
    [SerializeField] Transform cameraPos;
    [SerializeField] bool crouchPressed, isVulerable;
    [SerializeField] UnityEngine.UI.Image healthOverlayImage, sliderFillImage;

    [SerializeField] GameObject leftArmRig, rightArmRig, armParent;
  

    [SerializeField] Transform defaultLeftArmTarget, defaultRightArmTarget;

    [SerializeField] float regenSpeed;
    private PlayerData playerData;
    UnityEngine.UI.Slider slider;

    public bool down, movementEnabled;

    private float startingTransformHeight, startingControllerRadius, startingControllerHeight, moveSpeed,
    gravityMagnitude = -3f, staminaTime, timeSinceLastSprant, cameraBobOffset, regenVelocity = 0;

    private bool recovering, Grounded, isSprinting, sprintPressed, jumpPressed, badRecovering;

    private Vector3 startingCameraLocalPos, playerVelocity;

    private GameObject heldItem;
    [SerializeField] Transform cameraGuy;

    bool[] axisPriority = new bool[4];

    CharacterController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!IsOwner)
        {
            //this.GetComponent<MeshRenderer>().material = null;
            return;
        }

        playerData = GetComponent<PlayerData>();
        cameraGuy.gameObject.SetActive(true);
        health = startingHealth;

        staminaTime = defaultStamina;
        movementEnabled = true;
        controller = GetComponent<CharacterController>();

        slider = cameraGuy.GetComponentInChildren<UnityEngine.UI.Slider>();

        startingCameraLocalPos = cameraPos.localPosition;

        startingControllerHeight = controller.height;
        startingControllerRadius = controller.radius;
        startingTransformHeight = transform.localScale.y;

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cameraGuy.gameObject.SetActive(true);
            this.GetComponent<MeshRenderer>().enabled = false;
            //nameholder.SetActive(false);
        }
        else
        {
            cameraGuy.gameObject.GetComponent<AudioListener>().enabled = false;
            cameraGuy.gameObject.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        healthUpdate();
        if (movementEnabled)
        {
            processInput();
            movement();
            
        }

    }








    void healthUpdate()
    {

        timeSinceLastDamage += Time.deltaTime;
        if (health < startingHealth && timeSinceLastDamage > regenDelay)
        {
            float healthIntermediate = Mathf.SmoothDamp(health, startingHealth,ref regenVelocity,regenSpeed);

            health = healthIntermediate;
        }


        if (health > 148) { health = 150; }

        float alpha = Mathf.Abs((health / (float)startingHealth)-1);
        healthOverlayImage.color = new Color(255, 255, 255,alpha);

        if (health <= 0 && !down)
        {
            downed();
        }
    }

    void downed()
    {
        down = true;
        regenDelay = Mathf.Infinity;
        controller.height = startingControllerHeight / 3;
        controller.radius = startingControllerRadius * 4;
        transform.localScale = new Vector3(transform.localScale.x, startingTransformHeight / 3, transform.localScale.z);
        movementEnabled = false;
    }

    public void damage(int damage)
    {
        if (isVulerable)
        {
            isVulerable = false;
            health -= damage;
            timeSinceLastDamage = 0;
            Invoke(nameof(resetVulnerability),iFrames);
            //StartCoroutine(resetVulnerability());
        }

    }

    void resetVulnerability()
    {
        isVulerable = true;
    }


    void movement()
    {
        Grounded = controller.isGrounded;

        cameraBob();

        crouchFunc();

        sprintCheck();

        if (Grounded && jumpPressed) { }

        if (isSprinting) { moveSpeed = defaultMoveSpeed * sprintMult; }
        else if (crouchPressed) { moveSpeed = defaultMoveSpeed * crouchMult; }
        else { moveSpeed = defaultMoveSpeed; }

        Vector3 movement = transform.right *move.x + transform.forward *move.y;



        movement.Normalize();


        playerVelocity.y += gravityMagnitude * Time.deltaTime;

        controller.Move(movement * moveSpeed * Time.deltaTime);

        //controller.Move(playerVelocity * Time.deltaTime);

    }

    void crouchFunc()
    {
        if (crouchPressed)
        {

            transform.localScale = new Vector3(transform.localScale.x, startingTransformHeight / 2, transform.localScale.z);
            controller.height = startingControllerHeight / 2;
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startingTransformHeight, transform.localScale.z);
            controller.height = startingControllerHeight;
        }
    }

    void cameraBob()
    {
        if (controller.velocity != Vector3.zero)
        {


            cameraBobOffset =  math.remap(1, -1, cameraBobAmplitude, -cameraBobAmplitude, Mathf.Sin(Time.time * cameraBobSpeed * moveSpeed));
            //cameraBobOffset = Mathf.Clamp(cameraBobOffset, -cameraBobAmplitude, cameraBobAmplitude);
            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, cameraPos.localPosition.y + cameraBobOffset, cameraPos.localPosition.z);

        }
        else
        {
            cameraBobOffset = Mathf.Lerp(cameraPos.localPosition.y, startingCameraLocalPos.y,0.3f);
            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, cameraBobOffset, cameraPos.localPosition.z);

        }

    }



    void sprintCheck()
    {

        slider.value = staminaTime / defaultStamina;

        if (badRecovering)
        {
            sliderFillImage.color = Color.mediumVioletRed;
        }
        else if (slider.value < 0.3f)
        {
            sliderFillImage.color = Color.orange;
        }
        
        else
        {
            sliderFillImage.color = Color.green;
        }

        if (sprintPressed && staminaTime > 0.1f && !recovering && !badRecovering && move.y == 1 && !crouchPressed)
        {
            timeSinceLastSprant = 0;
            isSprinting = true;
            staminaTime -= Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            timeSinceLastSprant += Time.deltaTime;
            if (timeSinceLastSprant > recoveryCoolDown) { recovering = true; }
        }

        if (staminaTime < 0.1f)
        {
            badRecovering = true;
        }
        else if (staminaTime > 2)
        {
            badRecovering = false;
        }

        if ((recovering && staminaTime < 2) || ((timeSinceLastSprant > recoveryCoolDown) && staminaTime <= defaultStamina))
        {
            staminaTime += Time.deltaTime;
        }

        if ((timeSinceLastSprant > recoveryCoolDown))
        {
            recovering = false;
        }

    }

    void processInput()
    {

        crouchPressed = (Input.GetKey(crouch)) ? true : false;

        sprintPressed = (Input.GetKey(sprint)) ? true : false;

        jumpPressed = (Input.GetKey(jump)) ? true : false;


        // logs the direction of the last direction button that was pressed, so that the player can override a movement by pressing the opposite key
        if (Input.GetKeyDown(forward))
        {
            axisPriority[0] = true;

            axisPriority[1] = false;
        }
        else if (Input.GetKeyDown(backward))
        {
            axisPriority[1] = true;
            axisPriority[0] = false;
        }

        if (Input.GetKeyDown(right))
        {
            axisPriority[2] = true;
            axisPriority[3] = false;
        }
        else if (Input.GetKeyDown(left))
        {
            axisPriority[3] = true;
            axisPriority[2] = false;
        }

        // if opposing directions are both pressed prioritise the newest input
        if ((Input.GetKey(forward) && !Input.GetKey(backward)) || (Input.GetKey(forward) && Input.GetKey(backward) && axisPriority[0]))
        {
            move.y = 1;
        }
        else if ((Input.GetKey(backward) && !Input.GetKey(forward)) || (Input.GetKey(forward) && Input.GetKey(backward) && axisPriority[1]))
        {
            move.y = -1;
        }
        else
        {
            move.y = 0;
        }


        // if opposing directions are both pressed prioritise the newest input
        if ((Input.GetKey(right) && !Input.GetKey(left)) || (Input.GetKey(right) && Input.GetKey(left) && axisPriority[2]))
        {
            move.x = 1;
        }
        else if ((Input.GetKey(left) && !Input.GetKey(right) || (Input.GetKey(left) && Input.GetKey(right) && axisPriority[3])))
        {
            move.x = -1;
        }
        else
        {
            move.x = 0;
        }

        if (Input.GetKey(KeyCode.E) && heldItem == null)
        {
            pickUpItem();
        }

        if (Input.GetKeyDown(KeyCode.G) && heldItem != null && !heldItem.transform.GetChild(0).GetComponent<gun>().isAnimating)
        {
            dropItem();
        }
    }

    void pickUpItem()
    {
        Debug.DrawRay(cameraPos.position, cameraPos.forward, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out hit, 500))
        {
            if (hit.transform.CompareTag("pickupable"))
            {
                heldItem = hit.transform.gameObject;

                hit.transform.GetChild(0).GetComponent<gun>().interaction(this.gameObject, cameraPos.GetComponent<camera>().gunPos.transform, cameraPos.GetComponent<camera>().gunAimPos.transform);

                hit.transform.SetParent(cameraPos);

                hit.transform.position = cameraPos.GetComponent<camera>().gunPos.transform.position;

                hit.transform.rotation = cameraPos.GetComponent<camera>().gunPos.transform.rotation;

                hit.transform.GetComponent<Rigidbody>().isKinematic = true;

                hit.transform.GetComponent<BoxCollider>().isTrigger = true;

                leftArmRig.GetComponent<ChainIKConstraint>().data.target = hit.transform.GetChild(1).transform;
                rightArmRig.GetComponent<ChainIKConstraint>().data.target = hit.transform.GetChild(2).transform;

                armParent.gameObject.SetActive(false);
                armParent.gameObject.SetActive(true);

            }

        }
    }

    void dropItem()
    {
        heldItem.transform.GetChild(0).transform.GetComponent<gun>().owner = null;
        heldItem.transform.GetComponent<Rigidbody>().isKinematic = false;
        heldItem.transform.GetComponent<BoxCollider>().isTrigger = false;

        heldItem.transform.SetParent(null);

        heldItem = null;
        leftArmRig.GetComponent<ChainIKConstraint>().data.target = defaultLeftArmTarget;
        rightArmRig.GetComponent<ChainIKConstraint>().data.target = defaultRightArmTarget;


        armParent.gameObject.SetActive(false);
        armParent.gameObject.SetActive(true);
    }


}
