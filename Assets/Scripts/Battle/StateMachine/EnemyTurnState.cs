using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnState : State
{

    public override void OnEnterState()
    {
        LevelManager.Instance.RenderEnemyDamage();
        Debug.Log("Enemy turn!");
    }

    public override void OnExitState()
    {
        Debug.Log("Dealt damage. No longer enemy.");
    }

}
