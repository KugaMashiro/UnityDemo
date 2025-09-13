using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionToParent : MonoBehaviour
{
    private Animator _animator;
    private Transform _parentTransform;
    private Vector3 _initLocalPos;
    private Quaternion _initLocalRot;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _parentTransform = transform.parent;

        _initLocalPos = transform.localPosition;
        _initLocalRot = transform.localRotation;
    }

    private void OnAnimatorMove()
    {
        _parentTransform.Translate(_animator.deltaPosition, Space.World);
        _parentTransform.rotation *= _animator.deltaRotation;

        transform.localPosition = _initLocalPos;
        transform.localRotation = _initLocalRot;
    }
}
