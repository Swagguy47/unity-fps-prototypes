using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDisabledAnimChar : MonoBehaviour
{
    public void PlayAnim(AnimatedCharacter Char)
    {
        Char.gameObject.SetActive(true);
        Char.PlayScene();
    }
}
