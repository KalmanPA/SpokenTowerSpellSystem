using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellPickup : Pickup
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private Spell _spell;

    /// <summary>
    /// If no specific spell was set, it will pick a random spell in the Start mehtod
    /// </summary>
    private bool _specificSpellWasSet = false;

    private int _temporaryNumberOfUses = 0;

    protected override void Start()
    {
        base.Start();

        if (!_specificSpellWasSet)
        {
            _spell = SpellManager.Instance.GetRandomSpell();

            _spriteRenderer.sprite = _spell.SpellSprite;
        }
        
        
    }

    public void SetSpellToDroppedSpell(Spell spell, int numberOfUses)
    {
        _spell = spell;

        _spriteRenderer.sprite = _spell.SpellSprite;

        _temporaryNumberOfUses = numberOfUses;

        _specificSpellWasSet = true;
    }

    protected override void PickedUp()
    {
        SpellManager.Instance.InitializeNewHeldSpell(_spell, _temporaryNumberOfUses);

        base.PickedUp();
    }
}
