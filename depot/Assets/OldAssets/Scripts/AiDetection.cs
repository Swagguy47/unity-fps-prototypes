
using System.Collections.Generic;
using UnityEngine;

//This script could likely be optimized a lot
public class AiDetection : MonoBehaviour
{
    [SerializeField] private CharacterBrain Character;
    [SerializeField] private CharacterAI CharacterAi;

    private List<CharacterBrain> InRangeEnemies = new List<CharacterBrain>();
    private bool Looking = true;

    private Vector3 LastSeenPos;

    [SerializeField] private float CheckFrequency = 30;
    Collider[] colliders = new Collider[50];
    int count;
    float scanInterval;
    float scanTimer;
    List<GameObject> Objects = new List<GameObject>();
    public List<GameObject> NewObjects { //removes null objects
        get
        {
            Objects.RemoveAll(obj => !obj);
            return Objects;
        }
    }

    [Header("ViewconeGeneration")]
    [SerializeField] private float Distance = 10;
    [SerializeField] private float Angle = 30;
    [SerializeField] private float Height = 1;

    Mesh mesh;

    private void Start()
    {
        //GenerateViewconeMesh();
        OnValidate();
    }

    private void LateUpdate()
    {
        //Scanning
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();
        }

        //Character.gameObject.name = "Obj: " + Objects.Count + " State: " + CharacterAi.AiState + " Looking: " + Looking;

        if (CharacterAi.EnemyTarget == null)
        {
            if (Looking && NewObjects.Count > 0)
            {
                //If has line of sight to enemy, target it
                foreach (GameObject Obj in NewObjects)
                {
                    Debug.Log("CHECKING " + Obj.name);
                    if (Physics.Linecast(transform.position, Obj.transform.position) && Obj.layer == 6)
                    {
                        Debug.Log("PASSED LINE CHECK");
                        CharacterBrain Enemy = Obj.GetComponent<CharacterBrain>();
                        if (Enemy.CurrentTeam != Character.CurrentTeam)
                        {
                            CharacterAi.PathAwait = 0;
                            Looking = false;
                            CharacterAi.EnemyTarget = Enemy;
                            CharacterAi.AiState = 7; //spotted animation
                            Debug.Log("Enemy Spotted!");
                        }
                    }
                }
            }
        }
        else
        {
            //Remembers the last location of the enemy
            if (Physics.Linecast(transform.position, CharacterAi.EnemyTarget.transform.position))
            {
                LastSeenPos = CharacterAi.EnemyTarget.transform.position;
            }
            else //Investigates last known location if killed/lost enemy
            {
                //Character.AiInvestigate(LastSeenPos);
                Character.AiAwait = true;
                CharacterAi.EnemyTarget = null;

                Looking = true;
            }
        }
    }

    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, Distance, colliders);

        Objects.Clear();
        for (int i = 0; i < count; ++i)
        {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj))
            {
                Objects.Add(obj);
            }
        }
    }

    public bool IsInSight(GameObject obj)
    {
        Vector3 origin = transform.position;
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;
        if (direction.y < 0 || direction.y > Height)
        {
            return false;
        }

        direction.y = 0;
        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > Angle)
        {
            return false;
        }

        origin.y += Height / 2;
        dest.y = origin.y;
        if (Physics.Linecast(origin, dest))
        {
            return false;
        }

        if (obj.layer != 6)
        {
            //return false;
        }

        return true;
    }

    Mesh GenerateViewconeMesh()
    {
        Mesh mesh = new Mesh();

        int numTriangles = 8;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] Triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -Angle, 0) * Vector3.forward * Distance;
        Vector3 bottomRight = Quaternion.Euler(0, Angle, 0) * Vector3.forward * Distance;
        
        Vector3 topCenter = bottomCenter + Vector3.up * Height;
        Vector3 topLeft = bottomLeft + Vector3.up * Height;
        Vector3 topRight = bottomRight + Vector3.up * Height;

        int vert = 0;

        // Left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        // Right side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        // Far side
        vertices[vert++] = bottomLeft;
        vertices[vert++] = bottomRight;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = topLeft;
        vertices[vert++] = bottomLeft;

        // top
        vertices[vert++] = topCenter;
        vertices[vert++] = topLeft;
        vertices[vert++] = topRight;

        // bottom
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomLeft;

        for (int i = 0; i < numVertices; ++i)
        {
            Triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = Triangles; 
        mesh.RecalculateNormals();
        return mesh;
    }

    private void OnValidate()
    {
        mesh = GenerateViewconeMesh();
    }

    private void OnDrawGizmos()
    {
        if (mesh)
        {
            Gizmos.color = new Color(0,3,6,0.2f);
            //Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            CharacterBrain Char = other.gameObject.GetComponent<CharacterBrain>();

            //if (Char.CurrentTeam != Character.CurrentTeam)
            //{
            //    InRangeEnemies.Add(Char);
            //}
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            CharacterBrain Char = other.gameObject.GetComponent<CharacterBrain>();

            //if (Char.CurrentTeam != Character.CurrentTeam)
            //{
            //    InRangeEnemies.Remove(Char);
            //}
        }
    }


}
