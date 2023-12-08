using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SecurityGuard;

public class ForceOpen : MonoBehaviour
{
    private SecurityGuard securityGuard;
    private GameObject guardObject;
    private GameObject sprite;
    private bool isOpen = false;
    void Awake()
    {
        securityGuard = transform.parent.GetComponent<SecurityGuard>();
        guardObject = transform.parent.gameObject;
        sprite = guardObject.transform.Find("Sprite").gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Character") || collision.CompareTag("Player"))
        {
            //Room is not open yet. This means the player has to get in the room, then it will close again.
            if (!isOpen)
            {
                isOpen = true;
                BoxCollider2D collider = GetComponent<BoxCollider2D>();
                collider.size = new Vector2(2, 1);
                collider.offset = new Vector2(0, -2);
                //Disable the guard collider
                guardObject.GetComponent<BoxCollider2D>().enabled = false;
                switch (securityGuard.guardPosition)
                {
                    case GuardPosition.Left:
                        sprite.transform.localPosition = new Vector3(transform.position.x + 0.75f, transform.position.y - 1.5f, transform.position.z);
                        break;
                    case GuardPosition.Right:
                        sprite.transform.localPosition = new Vector3(transform.position.x - 0.75f, transform.position.y + 1.5f, transform.position.z);
                        break;
                    case GuardPosition.Up:
                        sprite.transform.localPosition = new Vector3(transform.position.x - 1.5f, transform.position.y - 0.75f, transform.position.z);
                        break;
                    case GuardPosition.Down:
                        sprite.transform.localPosition = new Vector3(transform.position.x + 1.5f, transform.position.y + 0.75f, transform.position.z);
                        break;
                }
            }
            else
            {
                isOpen = false;
                guardObject.GetComponent<BoxCollider2D>().enabled = true;
                sprite.transform.localPosition = Vector3.zero;
                //Destroy collider
                Destroy(gameObject);
            }
        }
    }
}
