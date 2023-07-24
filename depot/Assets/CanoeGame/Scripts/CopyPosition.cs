using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosition : MonoBehaviour
{
    [SerializeField] Transform Copy;
    [SerializeField] bool LockX, LockY, LockZ;
    Vector3 SavedPos;

    private void Start()
    {
        SavedPos= transform.position;
    }
    private void Update()
    {
        transform.position = new Vector3((SavedPos.x * System.Convert.ToInt32(LockX)) + (Copy.position.x * System.Convert.ToInt32(!LockX)), (SavedPos.y * System.Convert.ToInt32(LockY)) + (Copy.position.y * System.Convert.ToInt32(!LockY)), (SavedPos.z * System.Convert.ToInt32(LockZ)) + (Copy.position.z * System.Convert.ToInt32(!LockZ)));
    }
}
