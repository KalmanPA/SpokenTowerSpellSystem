using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpell
{
    /// <summary>
    /// Checks if the spell's function is executable in the current context of the game
    /// </summary>
    /// <returns></returns>
    public bool CanSpellLogicRun();
}
