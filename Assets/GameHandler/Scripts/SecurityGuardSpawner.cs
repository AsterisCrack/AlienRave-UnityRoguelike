using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityGuardSpawner : MonoBehaviour
{
    public static SecurityGuardSpawner instance;
    public GameObject securityGuardPrefab;

    public void Awake()
    {
        instance = this;
    }

    public SecurityGuard CreateGuard(Vector2 pos, SecurityGuard.GuardPosition guardPosition, Node node)
    {
        GameObject guard = Instantiate(securityGuardPrefab, pos, Quaternion.identity);
        guard.transform.position = pos;
        SecurityGuard guardScript = guard.GetComponent<SecurityGuard>();
        guardScript.ChangePosition(guardPosition);
        return guardScript;
    }
}
