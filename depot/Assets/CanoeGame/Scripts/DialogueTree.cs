using UnityEngine;

[CreateAssetMenu(fileName = "DialogueTree", menuName = "ArcticGame/Dialogue")]
public class DialogueTree : ScriptableObject
{
    [Header("Character Speech")]
    public string CharacterSpeech = "Hello";
    public float SpeechTimeMult = 1;

    [Tooltip("Leave string blank to ignore option, leave scriptableObejct blank to exit dialogue")]
    [Header("Response Options")]
    public string Response1;
    public DialogueTree Tree1;
    public AnimatorOverrideController AnimOverride1;

    public string Response2;
    public DialogueTree Tree2;
    public AnimatorOverrideController AnimOverride2;

    public string Response3;
    public DialogueTree Tree3;
    public AnimatorOverrideController AnimOverride3;

    public string Response4;
    public DialogueTree Tree4;
    public AnimatorOverrideController AnimOverride4;
}
