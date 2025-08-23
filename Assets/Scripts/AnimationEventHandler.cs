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

    public void OnAnimInteractWindowOpen()
    {
        //Debug.Log("Anim Combo Window Open");
        EventCenter.PublishAnimInteractWindowOpen();
    }

    public void OnAnimChargeStart()
    {
        //Debug.Log("Anim Charge Start");
        EventCenter.PublishAnimChargeStart();
    }

    public void OnAnimChargeEnd()
    {
        //Debug.Log("Anim Charge End");
        EventCenter.PublishAnimChargeEnd();
    }
}