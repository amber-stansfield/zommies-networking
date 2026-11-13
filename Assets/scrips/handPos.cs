using UnityEngine;

public class handPos : MonoBehaviour
{
    [SerializeField] Transform HandIkTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position,HandIkTarget.position, 0.25f);
    }
}
