using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{

    private static BattleManager _instance;
    public static BattleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<BattleManager>();
            }
            return _instance;
        }
    }

    #region Singleton Logic
    private void Awake()
    {
        if (!ReferenceEquals(_instance, this)) {
            if (_instance != null)
            {
                Destroy(this);
            }
            else
            {
                _instance = this;
            }
        }
    }
    #endregion

    [Header("Object Assignments")]
    public PlayerHandler PlayerHandler;
    public EnemyHandler CurrEnemyHandler;
    [SerializeField] private EnemyInfoBox _enemyInfoBox;

    public State CurrentState;

    public Action OnPlayerAttack = null;
    public Action OnEnemyAttack = null;
    public Action<State> OnStateChanged = null;  // Parameter is the state to transition to
    public Action OnReachedLastEnemy = null;

    private void Start()
    {
        SetState(new PlayerTurnState()); // Start off as player turn
        SetNewEnemy(CurrEnemyHandler);
    }

    public void SetState(State s)
    {
        OnStateChanged?.Invoke(s);
        CurrentState?.OnExitState();  // Exit from the current state, if any
        CurrentState = s;
        CurrentState.OnEnterState();
    }

    /// <summary>
    /// Render a new enemy, updating any UI if needed.
    /// Also queues any dialogue to be played if applicable.
    /// </summary>
    public void SetNewEnemy(EnemyHandler newEnemyHandler)
    {
        CurrEnemyHandler = newEnemyHandler;
        _enemyInfoBox.SetInfo((EnemyData)(newEnemyHandler.CharData));
        // If there's any dialogue to play, play it!
        if (newEnemyHandler.DialogueToPlayOnMeet.Count > 0)
        {
            StartCoroutine(RenderDialogueCoroutine(newEnemyHandler.DialogueToPlayOnMeet));
        }
    }

    private IEnumerator RenderDialogueCoroutine(List<DialogueInfo> diList)
    {
        bool wasStalled = false;

        while (diList.Count > 0)
        {
            DialogueInfo di = diList[0];
            diList.RemoveAt(0);
            if (di.ShouldStallState)
            {
                wasStalled = true;
                SetState(new WaitState());
            }
            if (di.Speaker == Faction.PLAYER)
            {
                PlayerHandler.SayDialogue(di);
            }
            else if (di.Speaker == Faction.ENEMY)
            {
                CurrEnemyHandler.SayDialogue(di);
            }
            // Wait until the dialogue is finished, before the next one
            bool dialogueFinished = false;
            DialogueBoxHandler.OnDialogueComplete = () => {
                dialogueFinished = true; 
            };
            yield return new WaitUntil(() => dialogueFinished);
        }

        // If the dialogue was stalled, restore the state at the end
        if (wasStalled)
        {
            SetState(new PlayerTurnState());
        }
    }

    /// <summary>
    /// Deal damage (as the player) to the enemy.
    /// </summary>
    public void RenderAttackAgainstEnemy(int damage)
    {
        // Make the enemy actually take the damage.
        CurrEnemyHandler.HealthHandler.TakeDamage(damage);
    }

    /// <summary>
    /// Render damage (as the enemy) to the player.
    /// </summary>
    public void RenderAttackAgainstPlayer(EnemyAttack attack)
    {
        // If any effects should be applied, apply them.
        foreach (AttackStatus effect in attack.InflictedStatuses)
        {
            if (effect.Target == Faction.ENEMY)
            {
                CurrEnemyHandler.StatusHandler.GainStatusEffect(effect.Status, effect.Amplifier);
            }
            if (effect.Target == Faction.PLAYER)
            {
                PlayerHandler.StatusHandler.GainStatusEffect(effect.Status, effect.Amplifier);
            }
        }
        // Make the player actually take the damage.
        PlayerHandler.HealthHandler.TakeDamage(attack.Damage);
    }

    /// <summary>
    /// Load the current scene *again* after a certain delay.
    /// </summary>
    public void ReloadCurrentLevel(int delayInSecs)
    {
        Invoke(nameof(LoadLevel), delayInSecs);
    }

    private void LoadLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    /// <summary>
    /// Run specific code after the next frame, to avoid
    /// conflicts that occur due to priority.
    /// </summary>
    public void RunNextFrame(Action codeToRunAfter)
    {
        StartCoroutine(RunNextFrameCoroutine(codeToRunAfter));
    }

    private IEnumerator RunNextFrameCoroutine(Action codeToRunAfter)
    {
        yield return new WaitForEndOfFrame();
        codeToRunAfter.Invoke();
    }

}