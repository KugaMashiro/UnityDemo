using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public void OnAnimRollEnd()
    {
        //Debug.Log("Anim Roll End");
        EventCenter.PublicAnimRollEnd();
    }

    public void OnAnimAtkEnd()
    {
        EventCenter.PublicAnimAtkEnd();
    }
}