using System;
using Unity.Netcode;
using UnityEngine;

public class ikHolder : NetworkBehaviour
{
    [SerializeField] Transform camera;
    [SerializeField] Transform cameraPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!IsLocalPlayer) { camera = cameraPos; }
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = camera.position;
        this.transform.eulerAngles = new Vector3 (0.0f, camera.eulerAngles.y, 0.0f);
    }
}
