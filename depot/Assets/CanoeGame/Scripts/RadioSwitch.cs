using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioSwitch : MonoBehaviour
{
    Radio R;

    private void Start()
    {
        R = GetComponent<Radio>();
    }
    public void RadioToggle()
    {
        R.enabled= !R.enabled;
    }
}
