using UnityEngine;

public class ikHolder : MonoBehaviour
{
    private Transform camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = camera.transform.position;
        this.transform.eulerAngles = new Vector3 (0.0f, camera.transform.eulerAngles.y, 0.0f);
    }
}
