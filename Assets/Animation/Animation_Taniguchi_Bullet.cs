using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Animation_Taniguchi_Bullet : MonoBehaviour
{
    [SerializeField] GameObject mainSprite;

    float animationDuration = 0.33f;
    Vector3 original;
    private void Awake()
    {
        original = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = new Vector3(1, 0, 1);
        mainSprite.transform.localPosition = Vector3.zero;
        transform.DOScale(original, animationDuration).OnComplete(() =>
        {
            transform.DOScale(new Vector3(1, 0, 1), animationDuration);
            mainSprite.transform.DOLocalMoveY(0f, animationDuration);
        });
        mainSprite.transform.DOLocalMoveY(0.5f, animationDuration);
    }
    private void OnDisable()
    {
        transform.DORewind();
        mainSprite.transform.DORewind();
    }

}
