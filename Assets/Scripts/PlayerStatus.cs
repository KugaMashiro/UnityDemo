using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Player Properties")]
    [SerializeField] private int maxHealthPoint = 100;
    [SerializeField] private int maxStaminaPoint = 50;
    public float faceRotateSpeed { get; private set; } = 100f; 
    public float moveSpeed { get; private set; } = 5f;

    private int _curHealthPoint;
    private int _curStaminaPoint;
    private bool _isInvincible;
    private bool _canInteract = true;

    public int curHealthPoint => _curHealthPoint;
    public int curStaminaPoint => _curStaminaPoint;
    public bool isInvincible => _isInvincible;
    public bool canInteract => _canInteract;

    private void Awake() 
    {
        _curHealthPoint = maxHealthPoint;
        _curStaminaPoint = maxStaminaPoint;
    }    
}
