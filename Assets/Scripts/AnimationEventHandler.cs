using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public void OnAnimRollEnd()
    {
        //Debug.Log("Anim Roll End");
        EventCenter.PublishAnimRollEnd();
    }

    public void OnAnimAtkEnd()
    {
        EventCenter.PublishAnimAtkEnd();
    }

    public void OnAnimComboWindowOpen()
    {
        //Debug.Log("Anim Combo Window Open");
        EventCenter.PublishAnimComboWindowOpen();
    }
}