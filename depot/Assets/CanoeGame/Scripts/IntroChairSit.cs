using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroChairSit : MonoBehaviour
{
    bool Ready;
    [SerializeField] NpcAnimFwd DadCharacterAnims;
    [SerializeField] AnimatedCharacter FpCharacter;
    [SerializeField] IntroSeqManager SeqManager;

    public void Sit()
    {
        if (!DadCharacterAnims.Override)
        {
            FpCharacter.gameObject.SetActive(true);
            FpCharacter.PlayScene();
            SeqManager.Sequence = 2;
            SeqManager.CheckSequence();
            this.gameObject.SetActive(false);
        }
    }
}
