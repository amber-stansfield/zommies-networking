using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class targetFollower : MonoBehaviour
{
    public Transform target;
    private float distance, scootStartTime, scootTime, maxScootTime = 1.6f;
    public float maxDistance;
    [SerializeField] bool scooting;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ((distcheck(target) > maxDistance) && !scooting)
        {
            StartCoroutine(scoot());
        }
        if (scooting)
        {
            scootTime += Time.deltaTime;
            if (scootTime > maxScootTime)
            {
                scooting = false;
                scootTime = 0;
            }
        }
        else
        {
            scootTime = 0;
        }
    }

    private IEnumerator scoot()
    {
        scooting = true;
        float timer = 0;
        Transform lastpos = target;
        Vector3 startingPos = this.transform.position;
        Vector3 direction = (target.position - startingPos).normalized;
        Vector3 overshootPos = target.position + direction / 3;
        while (timer < 1)
        {

            this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, startingPos.y + 0.13f, this.transform.position.z), timer);
            this.transform.position = Vector3.Lerp(this.transform.position, overshootPos, timer);
            timer += 0.15f;
            yield return new WaitForSeconds(0.01f);
        }
        timer = 0;
        while (timer < 1)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, lastpos.position.y, this.transform.position.z), timer);
            timer += 0.1f;
            yield return new WaitForSeconds(0.01f);
        }
        //yield return new WaitForSeconds(0.3f);
        scooting = false;
    }


    private float distcheck(Transform target)
    {
        distance = Vector3.Distance(target.position, this.transform.position);
        return distance;
    }

}
