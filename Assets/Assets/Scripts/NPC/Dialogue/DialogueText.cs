using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/New Dialogue Container")]
public class DialogueText : ScriptableObject
{
    public string SpeakerName;

    [Header("Audio (Stile Undertale)")]
    [Tooltip("Il suono specifico per la 'voce' di questo personaggio. Lascia vuoto per usare quello di default.")]
    public AudioClip voiceSound;

    [TextArea(5, 10)]
    public string[] paragraphs;
}