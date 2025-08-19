using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Player Properties")]
    [SerializeField] private int MaxHealthPoint = 100;
    [SerializeField] private int MaxStaminaPoint = 50;
    [SerializeField] public float RollDistance { get; } = 3f;
    [SerializeField] public float JumpBackDistance { get; } = 1f;
    public float FaceRotateSpeed { get; private set; } = 100f; 
    public float WalkSpeed { get; private set; } = 3f;
    public float RunSpeed { get; private set; } = 5f;


    private int _curHealthPoint;
    private int _curStaminaPoint;
    private bool _isInvincible;
    private bool _canInteract = true;

    public int CurHealthPoint => _curHealthPoint;
    public int CurStaminaPoint => _curStaminaPoint;
    public bool IsInvincible => _isInvincible;
    public bool CanInteract => _canInteract;

    private void Awake() 
    {
        _curHealthPoint = MaxHealthPoint;
        _curStaminaPoint = MaxStaminaPoint;
    }    
}
