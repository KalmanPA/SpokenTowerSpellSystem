using player;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// This class is responsible for holding and removing spells, calling the spells effects and informing the Spell UI.
/// </summary>
public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance;

    public delegate void SpellCast(Spell spell, int remainingNumberOfUses, float cooldown);
    public static event SpellCast OnSpellCast;

    public delegate void RemoveSpell(bool isBeacauseOutOfUses);
    public static event RemoveSpell OnRemoveSpell;

    public delegate void AddSpell();
    public static event AddSpell OnAddSpell;

    private static GameObject _spellPickupPrefab;

    private  Spell[] _allSpells;

    private Spell _heldSpell;

    private int _heldSpellNumberOfUses;

    private float _heldSpellCooldown;

    private bool _isOnCooldown = false;

    private Coroutine _cooldownRoutine;

    #region INITIALIZE

    /// <summary>
    /// Loads all the Spells from the Resources folder
    /// </summary>
    public void InitializeSpells()
    {
        _allSpells = Resources.LoadAll<Spell>("Spells");//Loads spell objects

        // Load and assign a prefab using Addressables
        Addressables.LoadAssetAsync<GameObject>("SpellPrefab").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _spellPickupPrefab = handle.Result;
                Debug.Log("Spell prefab assigned successfully from Addressables.");
            }
            else
            {
                Debug.LogError("Failed to load spell prefab from Addressables.");
            }
        };
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple Spell managers");
        }
    }
    #endregion

    #region HOLD/REMOVE SPELL

    /// <summary>
    /// Get a random spell from the list of all spells
    /// </summary>
    public Spell GetRandomSpell()
    {
        return _allSpells[UnityEngine.Random.Range(0, _allSpells.Length)];
    }

    public Spell GetHeldSpell()
    {
        return _heldSpell;
    }

    /// <summary>
    /// Get the sprite of the currently Held spell
    /// </summary>
    public Sprite GetSpriteOfHeldSpell()
    {
        if(_heldSpell != null)
            return _heldSpell.SpellSprite;
        else return null;
    }

    /// <summary>
    /// Get the number of uses of the currently Held spell
    /// </summary>
    public int GetUsageNumberHeldSpell()
    {
        return _heldSpellNumberOfUses;
    }

    /// <summary>
    /// Assignes given spell to be the held spell
    /// </summary>
    /// <param name="spell">Spell to assign</param>
    /// <param name="numberOfUses">Optional, should be given if the number of uses is not the same as the spell's base number of uses</param>
    public void InitializeNewHeldSpell(Spell spell, int numberOfUses = 0)
    {
        // If the player is holding a spell it gets overwritten with the new one
        if (_heldSpell != null)
        {
            DropHeldSpell();
            RemoveHeldSpell(false);
        }

        _heldSpell = spell;

        //Checks if the numberOfUses can be used instead of the spell's base number of uses
        if (numberOfUses > 0 && numberOfUses <= spell.NumberOfUses)
        {
            _heldSpellNumberOfUses = numberOfUses;
        }
        else
        {
            _heldSpellNumberOfUses = spell.NumberOfUses;
        }

        _heldSpellCooldown = spell.Cooldown;

        OnAddSpell?.Invoke();
    }

    /// <summary>
    /// Creates a spell pickup and assignse the spell type of the dropped spell
    /// </summary>
    private void DropHeldSpell()
    {
        GameObject spellPickup =  Instantiate(_spellPickupPrefab, Player.Instance.transform.position, Quaternion.identity);

        spellPickup.GetComponent<SpellPickup>().SetSpellToDroppedSpell(_heldSpell, _heldSpellNumberOfUses);
    }

    /// <summary>
    /// Remove currently held spell
    /// </summary>
    /// <param name="isBecauseOutOfUses">This bool is to inform the UI if it came from replacment or becouse it run out of uses</param>
    private void RemoveHeldSpell(bool isBecauseOutOfUses)
    {
        _heldSpell = null;

        OnRemoveSpell?.Invoke(isBecauseOutOfUses);

        //Resets the cooldown
        if (_cooldownRoutine != null)
        {
            StopCoroutine(SpellCooldownRoutine());
            _cooldownRoutine = null;
        }
        
        _isOnCooldown = false;
    }
    #endregion

    #region CAST SPELL
    /// <summary>
    /// Activates the held spell's effect
    /// </summary>
    /// <returns>A bool that is true if the spell got cast or false if it did not got cast</returns>
    public bool CastHeldSpell()
    {
        //Check for cooldown and if the spell can be cast in the current context
        if (CheckIfSpellCanBeCast())
        {
            //Creartes the spell prefab with the spell logic
            Instantiate(_heldSpell.SpellObject, transform.position, Quaternion.identity);

            _heldSpellNumberOfUses--;

            if (_heldSpellNumberOfUses <= 0)
            {
                // Removes spell when there are no more uses
                RemoveHeldSpell(true);
                
            }
            else //Start cooldown
            {
                OnSpellCast?.Invoke(_heldSpell, _heldSpellNumberOfUses, _heldSpellCooldown);
                _cooldownRoutine = StartCoroutine(SpellCooldownRoutine());
            }

            return true;
        }
        else
        {
            return false;
        }

    }

    private bool CheckIfSpellCanBeCast()
    {
        //Checks if there is a held spell
        if (_heldSpell == null)
        {
            Debug.LogWarning("No Held Spell");
            return false;
        }

        // Checks if the held spell is on cooldown
        if (_isOnCooldown)
        {
            Debug.LogWarning("Spell is on cooldown");
            return false;
        }

        // Checks if the spell's function is executable in the current context of the game
        if (!_heldSpell.SpellObject.GetComponent<ISpell>().CanSpellLogicRun())
        {
            Debug.LogWarning("Spell cannot happen in the current context of the game");
            return false;
        }

        return true;
    }

    private IEnumerator SpellCooldownRoutine()
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(_heldSpellCooldown);
        _isOnCooldown = false;
        _cooldownRoutine = null;
    }
    #endregion
}
