using UnityEngine;
using UnityEngine.AI;

public class zommieBrain : MonoBehaviour
{

    NavMeshAgent brain;

    [SerializeField] float attackRange;
    [SerializeField] float attackCoolDown;
    private GameObject nearestGuy;

    private int damageAmount = 50;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        brain = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        nearestGuy = getNearestPlayer();
        brain.SetDestination(nearestGuy.transform.position);

        attack();

    }


    void attack()
    {
        if (Vector3.Distance(transform.position,nearestGuy.transform.position) < attackRange && nearestGuy.GetComponentInChildren<playerGamer>() != null)
        {
            Debug.Log($"attacked {nearestGuy.name}");
            nearestGuy.GetComponent<playerGamer>().damage(damageAmount);
        }
    }


    GameObject getNearestPlayer()
    {
        float dist;
        float shortestDist = Mathf.Infinity;
        CharacterController shortestGuy = null;
        GameObject output = null;
        foreach (CharacterController guy in FindObjectsByType<CharacterController>())
        {
            dist = Vector3.Distance(guy.gameObject.transform.position, this.transform.position);
            if (dist < shortestDist && !guy.gameObject.GetComponent<playerGamer>().down)
            {
                shortestGuy = guy;
                shortestDist = dist;
            }
        }
        if (shortestGuy == null)
        {
            output = GameObject.Find("idleSpot");
        }
        else
        {
            output = shortestGuy.gameObject;
        }

            return output;
    }
}
