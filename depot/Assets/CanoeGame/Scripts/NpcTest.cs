using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcTest : MonoBehaviour
{
    /*
    _____________________________________________________________
                            G E N E R A L
    -sleep at night
    random daily task:
    {
    -job (if applicable)
    -go to random spot and fish
    -go to shop / resturaunt
    -go to random farm
    -stay at home
    }

    -determine when they need to start returning home based off distance and time till sunset
    -go to bed when home and past curfew
    -random wake up time
    -run and hide from robots if at night
    -go to personal boat if destination is not reachable by land
    -tie up or untie boat when entering / exiting
    _____________________________________________________________
                            C U L T I S T S
    -bool for if they're a cultist
    random cultist tasks:
    {
    -job (if applicable)
    -go to random spot and fish
    -go to cult shop / resuraunt
    -go into houses in the day and take random item(s)
    -stay at home
    }
    -choose whether to return home near night or occasionally take risk and steal from houses / robot facilities
    -fight robots if found, ?teamwork?
    _____________________________________________________________
                          V A R I A B L E S
    -For basic functionality-
    transforms: job location, home idle location, home bed
    boatController: personal boat
    bool: cultist
    -For player talking-
    string: name
    dialogueTree: CharacterDialogue
     */

    [SerializeField] public Transform JobSpot, HomeIdleSpot, HomeBedSpot;
    [SerializeField] BoatController PersonalBoat;
    [SerializeField] bool Cultist, IgnoreTasks;
    [SerializeField] Animator Anims;
    [Header("Player Dialogue")]
    public string NpcName = "NAME ME!!!";

    //Private variables
    bool Asleep = true;
    int CurrentTask;
    NavMeshAgent AI;
    Vector3 LastBoatPos;
    //-1 = go to sleep
    //0 = stay at home
    //1 = go to work
    //2 = go fish
    //3 = go to shop / resturaunt
    //4 = go to farm

    [HideInInspector] public Transform LookAt;

    [HideInInspector] public bool Overriding;
    RuntimeAnimatorController fallbackAnimator;
    private void Start()
    {
        AI = GetComponent<NavMeshAgent>();
        fallbackAnimator = Anims.runtimeAnimatorController;
        
    }
    private void LateUpdate()
    {
        if (PlayerCallback.Weather.TimeRaw < 470 && PlayerCallback.Weather.TimeRaw > 715 && !IgnoreTasks) //Daily task
        {
            if (Asleep) {
                Asleep = false;
                CurrentTask = Random.Range(0, 4);
                DoTask();
            }
        }

        if (LookAt != null) { //look towards object (hacky, should be slerped)
            Anims.transform.parent.LookAt(LookAt);
            Anims.transform.parent.rotation = Quaternion.Euler(0, Anims.transform.parent.rotation.eulerAngles.y, 0);
        }
        else {
            Anims.transform.parent.rotation = Quaternion.Euler(0, 0, 0);
        }

        Anims.SetFloat("FwdSpeed", AI.velocity.magnitude);
    }

    private void DoTask()
    {
        if (CurrentTask == -1){ //eepy
            AI.SetDestination(HomeBedSpot.position);
            //AiSeeking.StartPath(transform.position, HomeBedSpot.position);
        }
        else if (CurrentTask == 0) { //chill at home
            AI.SetDestination(HomeIdleSpot.position);
            //AiSeeking.StartPath(transform.position, HomeIdleSpot.position);
        }
        else if (CurrentTask == 1) //work
        { 
            if (JobSpot != null && JobSpot != transform) //has job
            {
                AI.SetDestination(JobSpot.position);
                //AiSeeking.StartPath(transform.position, JobSpot.position);
            }
            else //failsafe for no job
            { 
                CurrentTask = Random.Range(0, 4);
                DoTask();
            }
        }
        else if (CurrentTask == 2) { //go fish
            AI.destination = transform.position + new Vector3(Random.Range(-150, 150), 0, Random.Range(-150, 150));
            //AiSeeking.StartPath(transform.position, transform.position + new Vector3(Random.Range(-150, 150), 0, Random.Range(-150, 150)));
        }
        else { //TODO
            CurrentTask = Random.Range(0, 4);
            DoTask();
        }
    }

    public void MoveTo(Vector3 pos)
    {
        //AI.destination = pos;
        AI.SetDestination(pos);
    }

    public void OverrideAnims(AnimatorOverrideController Override) //if called with override set to null return to fallback anims
    {
        if (Override != null) {
            Anims.runtimeAnimatorController = Override;
            Anims.SetTrigger("Override");
        }
        else {
            Anims.runtimeAnimatorController = fallbackAnimator;
        }
    }

    public void AllowExit(bool Allow)
    {
        Anims.SetBool("AllowExit",Allow);
    }
}
