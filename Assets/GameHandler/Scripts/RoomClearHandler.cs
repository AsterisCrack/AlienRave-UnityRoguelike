using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomClearHandler
{
    private int numEnemies;
    private List<SecurityGuard> guards;
    public RoomClearHandler(List<SecurityGuard> guards,List<EnemyHealthHandler> enemies, BossHealthHandler boss)
    {
        this.numEnemies = enemies.Count + (boss != null ? 1 : 0);
        this.guards = guards;
        //subscribe to events
        foreach (EnemyHealthHandler enemy in enemies)
        {
            enemy.OnEnemyDeath += EnemyDeath;
        }
        if (boss != null)
        {
            boss.OnEnemyDeath += EnemyDeath;
        }
    }

    private void EnemyDeath()
    {
        numEnemies--;
        if (numEnemies == 0)
        {
            RoomCleared();
        }
    }

    private void RoomCleared()
    {
        foreach (SecurityGuard guard in guards)
        {
            guard.RoomCleared();
        }
    }
}
