using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

//Script to handle player movement
//This is a top-down game, so the player will move in 4 directions + diagonals
//The player can also dash in the 4 directions + diagonals
public class PlayerMovement : MonoBehaviour
{
    //Inputs
    private PlayerInput playerInput;
    private InputAction move;
    private InputAction dash;
    private InputAction aim;

    //Variables
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    bool[] canMove = { true, true };
    float playerWidth;
    float playerHeight;
    private Vector3 mousePosition;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 movement;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 0.3f;

    private bool isDashing = false; public bool IsDashing { get { return isDashing; } }
    private bool isInmune = false; public bool IsInmune { get { return isInmune; } }
    private bool canDash = true;
    private float dashDistanceLeft;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        dashDistanceLeft = dashDistance;
        playerWidth = playerCollider.bounds.size.x;
        playerHeight = playerCollider.bounds.size.y;
    }

    void Awake()
    {
        //Get the inputs
        playerInput = GetComponent<PlayerInput>();
        move = playerInput.actions["Move"];
        dash = playerInput.actions["Dash"];
        aim = playerInput.actions["Aim"];
        //Assign check dash to the dash input
        dash.performed += ctx => CheckDash();
    }

    // Update is called once per frame
    void Update()
    {
        //If is inmune deactivate the collider
        if (isInmune)
        {
            playerCollider.enabled = false;
        }
        else
        {
            playerCollider.enabled = true;
        }
        //If is dashing, don't move
        if (isDashing)
        {
            return;
        }

        Move();
    }
    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        //Set the velocity to 0 to avoid sliding
        rb.velocity = Vector2.zero;
    }
    private bool[] CanMoveInDirection(Vector2 direction)
    {
        //Check if the player can move in the direction
        //This is done by casting a ray in the direction the player is trying to move
        //If the ray hits a wall, the player can't move
        Vector2 dirX = new Vector2(direction.x, 0);
        Vector2 dirY = new Vector2(0, direction.y);
        float distanceX = playerWidth / 2;
        float distanceY = playerHeight / 2;

        //Center of the body
        RaycastHit2D hitXCenter = Physics2D.Raycast(transform.position, dirX, distanceX, LayerMask.GetMask("CollidableWall"));
        RaycastHit2D hitYCenter = Physics2D.Raycast(transform.position, dirY, distanceY, LayerMask.GetMask("CollidableWall"));

        //sides of the body
        Vector3 playerSize = playerCollider.bounds.size;
        int movingHorizontal = direction.x != 0 ? 1 : 0;
        int movingVertical = direction.y != 0 ? 1 : 0;
        Vector3 ray1Pos = new Vector3(transform.position.x - ((playerSize.x / 2 - 0.01f) * movingVertical), transform.position.y - ((playerSize.y / 2 - 0.01f) * movingHorizontal), transform.position.z);
        Vector3 ray2Pos = new Vector3(transform.position.x + ((playerSize.x / 2 - 0.01f) * movingVertical), transform.position.y + ((playerSize.y / 2 - 0.01f) * movingHorizontal), transform.position.z);
        RaycastHit2D hitX1 = Physics2D.Raycast(ray1Pos, dirX, distanceX, LayerMask.GetMask("CollidableWall"));
        RaycastHit2D hitX2 = Physics2D.Raycast(ray2Pos, dirX, distanceX, LayerMask.GetMask("CollidableWall"));
        RaycastHit2D hitY1 = Physics2D.Raycast(ray1Pos, dirY, distanceY, LayerMask.GetMask("CollidableWall"));
        RaycastHit2D hitY2 = Physics2D.Raycast(ray2Pos, dirY, distanceY, LayerMask.GetMask("CollidableWall"));

        bool[] result = new bool[2];
        result[0] = hitXCenter.collider == null && hitX1.collider == null && hitX2.collider == null;
        result[1] = hitYCenter.collider == null && hitY1.collider == null && hitY2.collider == null;
        return result;
    }

    private void AnimateMovement(Vector2 dir)
    {
        
        //Check if it is not during a dash animation and wait till it is over
        List<string> dashingAnimNames = new List<string> { "RollUp", "RollDown", "RollRight", "RollRightUp",};

        //Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        if (!dashingAnimNames.Contains(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            //Check if it has children
            if (gameObject.GetComponentsInChildren<SpriteRenderer>().Length != 0)
            {
                //Set weapon to visible
                gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
            }

            //Set the animator parameters to correct animation
            //Only 2 variables are needed, the rest of the logic is handled in the animator
            //Player has to look at mouse position
            mousePosition = Camera.main.ScreenToWorldPoint(aim.ReadValue<Vector2>());
            float angle = Mathf.Atan2(mousePosition.y - transform.position.y, mousePosition.x - transform.position.x) * Mathf.Rad2Deg;
            angle = (angle + 360 + 90) % 360;

            //Flip if needed to animate correctly
            gameObject.GetComponent<SpriteRenderer>().flipX = (angle > 180) ? true : false;
            //Set the animator parameters
            if (angle > 180)
            {
                angle = 360 - angle;
            }

            animator.SetBool("IsDashing", false);
            animator.SetFloat("Angle", angle);
            animator.SetBool("Moving", (dir.x != 0 || dir.y != 0));
        }
        else
        {
            return;
        }
    }   

    private void Move()
    {   
        //Check where to move
        movement = move.ReadValue<Vector2>();
        //Set the animator parameters to correct animation
        AnimateMovement(movement);

        //Move to the required direction
        //Check if the player is not colliding with a wall
        canMove = CanMoveInDirection(movement);
        if (!canMove[0])
        {
            movement.x = 0;
        }
        if (!canMove[1])
        {
            movement.y = 0;
        }
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
    private void CheckDash()
    {
        if (canDash)
        {
            if (movement.x != 0 || movement.y != 0)
            {
                StartCoroutine(Dash(movement));
            }
        }
    }
    private float CanDashInDirection(Vector2 direction)
    {
        //Check if the player can Dash in the direction
        //return TerrainHeightmapSyncControl distance available to dash before hitting a wall
        float distance;
        float distanceX = playerWidth / 2;
        float distanceY = playerHeight / 2;
        float extraDistance;
        if (direction.x == 0)
        {
            extraDistance = distanceY;
        }
        else if (direction.y == 0)
        {
            extraDistance = distanceX;
        }
        else
        {
            extraDistance = math.sqrt(math.pow(distanceX, 2) + math.pow(distanceY, 2));
        }

        RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, direction, dashDistance, LayerMask.GetMask("CollidableWall"));
        if (raycastHit2D.collider == null)
        {
            distance =  dashDistance;
        }
        else if (raycastHit2D.distance < dashDistance)
        {
            distance = raycastHit2D.distance-extraDistance;
        }
        else
        {
            distance = dashDistance;
        }
        return distance;
    }
    private IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        isInmune = true;
        //Animate the dash
        AnimateDash(direction);
        
        canDash = false;
        direction.Normalize();
        dashDistanceLeft = CanDashInDirection(direction);
        float totalDashDistance = dashDistanceLeft;
        float time = Time.time;
        while (dashDistanceLeft > 0)
        {
            float nextDistance = dashSpeed * Time.deltaTime;
            if (nextDistance > dashDistanceLeft)
            {
                nextDistance = dashDistanceLeft;
            }
            transform.Translate(direction * nextDistance);
            dashDistanceLeft -= nextDistance;
            if (dashDistanceLeft <= totalDashDistance / 2)
            {
                isInmune = false;
            }
            yield return null;
        }
        rb.velocity = Vector2.zero;

        //deleay the remaining time. The dash lasts a total of 0.67 seconds
        float timeLeft = 0.67f - (Time.time - time);
        if (timeLeft > 0)
        {
            yield return new WaitForSeconds(timeLeft);
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void AnimateDash(Vector2 direction)
    {
        animator.SetBool("IsDashing", true);
        //Look for what angle is the dash
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle + 360 + 90) % 360;
        if (angle > 180)
        {
            //Flip if needed to animate correctly
            gameObject.GetComponent<SpriteRenderer>().flipX = true;
            angle = 360 - angle;
        }
        //Set the animator parameters
        animator.SetFloat("Angle", angle);

        //Check if it has children
        if (gameObject.GetComponentsInChildren<SpriteRenderer>().Length > 0)
        {
            //Set weapon to visible
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = false;
        }
    }
}
