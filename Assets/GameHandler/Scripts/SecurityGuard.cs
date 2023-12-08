using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityGuard : MonoBehaviour
{
    public enum GuardPosition { Left, Right, Up, Down };
    public GuardPosition guardPosition;

    private BoxCollider2D boxCollider2D;
    private GameObject openTrigger;
    private Animator animator;
    // Start is called before the first frame update
    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        openTrigger = transform.Find("ForceOpenTrigger").gameObject;
        boxCollider2D.enabled = true;
        animator = GetComponentInChildren<Animator>();
    }

    public void ChangePosition(GuardPosition guardPosition)
    {
        this.guardPosition = guardPosition;
        switch (guardPosition)
        {
            case GuardPosition.Left:
                boxCollider2D.size = new Vector2(1, 2);
                openTrigger.transform.localRotation = Quaternion.Euler(0, 0, 90);
                GetComponentInChildren<SpriteRenderer>().flipX = true;
                animator.SetFloat("Horizontal", 1);
                animator.SetFloat("Vertical", 0);
                break;
            case GuardPosition.Right:
                boxCollider2D.size = new Vector2(1, 2);
                openTrigger.transform.localRotation = Quaternion.Euler(0, 0, -90);
                animator.SetFloat("Horizontal", -1);
                animator.SetFloat("Vertical", 0);
                break;
            case GuardPosition.Up:
                boxCollider2D.size = new Vector2(2, 1);
                animator.SetFloat("Vertical", -1);
                animator.SetFloat("Horizontal", 0);
                break;
            case GuardPosition.Down:
                boxCollider2D.size = new Vector2(2, 1);
                openTrigger.transform.localRotation = Quaternion.Euler(0, 0, 180);
                animator.SetFloat("Vertical", 1);
                animator.SetFloat("Horizontal", 0);
                break;
        }
    }
    public void RoomCleared()
    {
        boxCollider2D.enabled = false;

        //Disable the trigger
        if (openTrigger != null) openTrigger.SetActive(false);

        switch (guardPosition)
        {
            case GuardPosition.Left:
                transform.position = new Vector3(transform.position.x + 0.75f, transform.position.y - 1.5f, transform.position.z);
                break;
            case GuardPosition.Right:
                transform.position = new Vector3(transform.position.x - 0.75f, transform.position.y + 1.5f, transform.position.z);
                break;
            case GuardPosition.Up:
                transform.position = new Vector3(transform.position.x -1.5f, transform.position.y - 0.75f, transform.position.z);
                break;
            case GuardPosition.Down:
                transform.position = new Vector3(transform.position.x + 1.5f, transform.position.y + 0.75f, transform.position.z);
                break;
        }
    }
}
