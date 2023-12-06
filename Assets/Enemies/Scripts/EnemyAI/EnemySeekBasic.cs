using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemySeekBasic : PlayerLocator
{
    private Transform target;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float nextWaypointDistance = 3f;
    [SerializeField] private float stoppingDistance = 4f;
    [SerializeField] private float threasholdStoppingDistance = 2f;
    private bool isStopped = false;

    [SerializeField] private bool lookAtPlayer = true;

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;

    private Seeker seeker;
    private Rigidbody2D rb;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        FindPlayer();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        target = player.transform;
        seeker.StartPath(rb.position, target.position, OnPathComplete);
        StartCoroutine(UpdatePathCoroutine());
    }

    IEnumerator UpdatePathCoroutine()
    {
        while (true)
        {
            UpdatePath();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdatePath()
    {
        if(seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (path == null)
        {
            return;
        }

        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        } else
        {
            reachedEndOfPath = false;
        }

        if (!isStopped) 
        {
            Move();
            CheckDistance();
            //Debug.Log(GetDistanceToPlayer() + " " + IsPlayerInSight());
            if (GetDistanceToPlayer() <= stoppingDistance && IsPlayerInSight())
            {
                isStopped = true;
            }
        }
        else
        {
            Animate(player.transform.position - transform.position);
            if (GetDistanceToPlayer() > stoppingDistance + threasholdStoppingDistance || !IsPlayerInSight())
            {
                isStopped = false;
            }
        }
    }

    private void CheckDistance()
    {
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
    private void Move()
    {
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        if (reachedEndOfPath)
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("Moving", false);
        }
        else
        {
            rb.AddForce(force);
            anim.SetBool("Moving", true);
        }
        Animate(direction);
    }
    private void Animate(Vector2 direction)
    {
        float angle = 0;
        if (lookAtPlayer)
        {
            Vector2 directionPlayer = (target.position - transform.position).normalized;
            angle = Mathf.Atan2(directionPlayer.y, directionPlayer.x) * Mathf.Rad2Deg;
        }
        else
        {
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        angle = (angle + 360 + 90) % 360;

        //Flip if needed to animate correctly
        gameObject.GetComponentInChildren<SpriteRenderer>().flipX = (angle > 180) ? true : false;
        //Set the animator parameters
        if (angle > 180)
        {
            angle = 360 - angle;
        }
        anim.SetFloat("Angle", angle);
    }
}
