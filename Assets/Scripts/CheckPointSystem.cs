using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : MonoBehaviour
{
    public static CheckPointSystem Instance { get; private set; }

    [Header("Pawn")]
    [SerializeField] private GameObject _pawn;

    [Header("Revive Transform")]
    [SerializeField] private Transform _reviveTransform;

    //private Animator _pawnAnimator;
    //private PlayerStateManager _stateManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //_stateManager = _pawn.GetComponent<PlayerStateManager>();
        //_pawnAnimator = _pawn.GetComponentInChildren<Animator>();
    }

    public void Revive()
    {
        _pawn.transform.position = _reviveTransform.position;
        _pawn.transform.rotation = _reviveTransform.rotation;

        InventoryManager.Instance.SupplementaryItems();
        //_pawnAnimator.SetTrigger(AnimParams.Trigger_Revive);
        EventCenter.PublishStateChange(PlayerStateType.Revive);
    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
