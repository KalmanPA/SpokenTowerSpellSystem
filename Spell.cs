using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "SpokenTower/Spell")]
public class Spell : ScriptableObject
{
    public float Cooldown;

    public int NumberOfUses;

    public Sprite SpellSprite;

    public GameObject SpellObject;
}
