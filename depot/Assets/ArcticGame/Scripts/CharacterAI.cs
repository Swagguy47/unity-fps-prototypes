using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : MonoBehaviour
{
    //Essential Components
    AIPath ThisAi;
    Seeker ThisSeek;
    CharacterBrain Brain;

    //Ai functions -------------------------------------------
    [HideInInspector] public int AiState;
    //0 - random valid state
    //1 - patrol to random nearby location
    //2 - rush to average position of all enemies (if it comes across a vehicle get in)
    //3 - crouch then go to state 0
    //4 - strafe left and right
    //5 - retreat to random position in opposite direction of current enemy target
    //6 - assault current enemy target by rushing towards them
    //7 - react to spotting an enemy then go to random combat state
    //8 - regroup with closest friendly ammo mule if low on ammo
    //9 - regroup with closest friendly medic if out of bandages & injured
    //10- retreat away from average of all enemies (only used for mission endings)

    [HideInInspector] public int AiAwareness;
    //0 - completely unaware                (Gun At Rest)
    //1 - suspicious/investigating          (Gun Held Down)
    //2 - in combat                         (Gun Aiming)
    [HideInInspector] public CharacterBrain EnemyTarget;
    //--------------------------------------------------------

    //Internal functionality
    //private bool AwaitPathEnd; //if true it won't continuously call initial AiAction functions
    [HideInInspector] public float PathAwait; //Timer for how long ai should follow path

    //External variables
    [SerializeField] private float MaxPatrolDistance = 30;
    [SerializeField] private float RushTargetInaccuracy = 15;
    [SerializeField] private AnimatorOverrideController[] ReactionAnimators;

    private void Start()
    {
        Brain = GetComponent<CharacterBrain>();
        ThisAi = GetComponent<AIPath>();
        ThisSeek = GetComponent<Seeker>();

        EnemyTarget = Brain; //stupid, becomes self destructive
    }

    public void UpdateAI()
    {
        if (AiState == 0) //if no state, assign one
        {
            AiState = UnityEngine.Random.Range(1, 3);
        }
        AiAction();
        //Debug.Log(AiState);
    }

    private void AiAction()
    {
        if (PathAwait <= 0)
        {
            //Initial AiAction stuff

            if (AiState == 1) //patrol to random point
            {
                PathAwait = 40;
                ThisSeek.StartPath(transform.position, transform.position + new Vector3(Random.Range(-MaxPatrolDistance, MaxPatrolDistance), 0, Random.Range(-MaxPatrolDistance, MaxPatrolDistance)));
                //AwaitPathEnd = true;
            }
            else if (AiState == 2) //Rush to enemy average positions
            {
                PathAwait = 10;
                if (Brain.CurrentTeam == 0)
                {
                    ThisSeek.StartPath(transform.position, Sandbox.Team1Avg + new Vector3(Random.Range(-RushTargetInaccuracy, RushTargetInaccuracy), 0, Random.Range(-RushTargetInaccuracy, RushTargetInaccuracy)));
                }
                else if (Brain.CurrentTeam == 1)
                {
                    ThisSeek.StartPath(transform.position, Sandbox.Team0Avg + new Vector3(Random.Range(-RushTargetInaccuracy, RushTargetInaccuracy), 0, Random.Range(-RushTargetInaccuracy, RushTargetInaccuracy)));
                }
            }
            else if (AiState == 3) //Crouch
            {
                AiState = 0;
                Debug.Log("Would've crouched");
            }
            else if (AiState == 4) //Strafe left and right
            {

            }
            else if (AiState == 5) //retreat to random position opposite of current enemy target
            {

            }
            else if (AiState == 6) //assault current enemy target, rushing towards them
            {
                PathAwait = 20;
                ThisSeek.StartPath(transform.position, transform.position + EnemyTarget.transform.position);

            }
            else if (AiState == 7) //react to spotting an enemy
            {
                Brain.Animated = true;
                Brain.CharacterAnimator.runtimeAnimatorController = ReactionAnimators[Random.Range(0, ReactionAnimators.Length)];
                Brain.CharacterAnimator.SetTrigger("Override");
                AiState = Random.Range(4, 6);
            }
            else if (AiState == 8) //regroup with closest friendly ammo mule
            {

            }
            else if (AiState == 9) //regroup with closest friendly medic
            {

            }
            else if (AiState == 10) //(END STATE) retreat away from average of all enemies
            {

            }
        }
        else if(ThisAi.reachedEndOfPath) //If they've reached their destination
        {
            PathAwait = 0;
            AiState = 0;
            //do any final AiAction steps here
        }
        else
        {
            PathAwait -= Time.deltaTime;
        }
    }
}
