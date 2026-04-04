using UnityEngine;

public class AnimationReceiver : MonoBehaviour
{
    public SamuraiAI samuraiScript;

    public void TriggerAttackHit()
    {
        if (samuraiScript != null)
        {
            samuraiScript.TriggerAttackHit();
        }
    }
}