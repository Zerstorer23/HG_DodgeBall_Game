using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_ProjectileFInder : MonoBehaviour
{
    [SerializeField] Unit_Player player;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        player.IncrementEvasion();
    }
}
