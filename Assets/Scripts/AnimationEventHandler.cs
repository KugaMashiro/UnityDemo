using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public void OnAnimRollEnd()
    {
        //Debug.Log("Anim Roll End");
        EventCenter.PublishAnimRollEnd();
    }

    public void OnAnimAtkEnd(int atkComboIndex)
    {
        EventCenter.PublishAnimAtkEnd(atkComboIndex);
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

    public void OnAnimAtkConsumeSP()
    {
        //Debug.Log("Atk consume SP");
        EventCenter.PublishAnimAtkConsumeSP();
    }

    public void OnAnimRotateWindowOpen()
    {
        EventCenter.PublishAnimRotateWindowOpen();
    }

    public void OnAnimRotateWindowClose()
    {
        EventCenter.PublishAnimRotateWindowClose();
    }

    public void OnAnimMoveWindowOpen()
    {
        Debug.Log("can move");
        EventCenter.PublishAnimMoveWindowOpen();
    }
}