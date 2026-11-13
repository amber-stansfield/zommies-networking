using UnityEngine;

public class legPlacement : MonoBehaviour
{
    public GameObject placeObj;
    public LayerMask mask;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, -transform.up, out hit, 500f, mask))
        {
            Vector3 hitpos = hit.point;
            placeObj.transform.position = hitpos;

        }
    }


   /* private void OnDrawGizmos()
    {
        Debug.DrawRay(this.transform.position, -Vector3.up,Color.red, 1);
    }*/
}
