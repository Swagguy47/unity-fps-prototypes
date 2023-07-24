using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProtoHackPanel : MonoBehaviour
{
    [SerializeField] bool RandomOnStart;
    [Range(1, 15)] public int Wavelength = 2;
    [Range(-7, 7)] public int Speed = 1;
    [SerializeField] ProtoHackingUI UI;
    [SerializeField] Weapon HackingTool;
    public UnityEvent HackState;

    private void Start()
    {
        if (RandomOnStart) {
            Wavelength = Random.Range(1, 15);
            Speed = Random.Range(-7, 7);
        }
    }
    public void BeginHacking()
    {
        if (PlayerCallback.Inventory.CountItem(HackingTool) > 0) {
            UI.StartHacking(this);
        }
    }
}
