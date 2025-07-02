using Audio;
using player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayParticleSystem))]
public class Regeneration_Spell : MonoBehaviour,ISpell
{
    [Header("Settings")]
    [SerializeField] private int _healthToRegenerate = 4;
    [SerializeField] private float _delayBetweenHealing = 1.5f;

    [SerializeField] private AudioClip _castSound;

    private GameObject _particleSystemInstance;

    private void Start()
    {
        _particleSystemInstance = this.GetComponent<PlayParticleSystem>().PlayParticleEffect(Player.Instance.transform, new Vector2(0.0f, -0.47f));
        StartCoroutine(HealOverTime());
    }

    private IEnumerator HealOverTime()
    {
        AudioPlayer.Instance.PlaySoundEffect(_castSound);

        for (int i = 0; i < _healthToRegenerate; i++)
        {
            HealPlayer();
            yield return new WaitForSeconds(_delayBetweenHealing);
        }

        Destroy(_particleSystemInstance);
        Destroy(gameObject);
    }

    private void HealPlayer()
    {
        Player_Health.Heal(1);
    }

    public bool CanSpellLogicRun()
    {
        if (Player_Health.Current_Health < Player_Health.Current_Max_Health)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
