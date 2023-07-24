using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Contains arrays of all characters and general info about them

public class Sandbox : MonoBehaviour
{
    public List<CharacterBrain> Team0;
    public List<CharacterBrain> Team1;

    [HideInInspector] public static Vector3 Team0Avg, Team1Avg;

    private void Start()
    {
        RecalculateTeams();
    }

    private void LateUpdate()
    {
        RecalculateTeams(); //suboptimal
        Team0Avg = ReturnAverage(Team0);
        Team1Avg = ReturnAverage(Team1);
    }

    private Vector3 ReturnAverage(List<CharacterBrain> pos) //gets average position of characters on a side, for easier Ai pathfinding
    {
        Vector3 Avg = Vector3.zero;

        foreach (CharacterBrain Current in pos)
        {
            Avg += Current.transform.position;
        }

        return (Avg / pos.Count);
    }

    public void RecalculateTeams() //inefficient, do not use in hot path. clears the team lists and refinds all Characters for them
    {
        Team0.Clear();
        Team1.Clear();

        foreach (CharacterBrain brain in GameObject.FindObjectsOfType<CharacterBrain>())
        {
            if (brain.CurrentTeam == 0)
            {
                Team0.Add(brain);
            }
            else if (brain.CurrentTeam == 1)
            {
                Team1.Add(brain);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Team0Avg, 2);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere (Team1Avg, 2);
    }
}
