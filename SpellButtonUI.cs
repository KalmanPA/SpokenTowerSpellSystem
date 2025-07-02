using Audio;
using Behavior.Properties;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TimeScale;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Animator_SpellButton), typeof(AdjustOpacity))]
    public class SpellButtonUI : MonoBehaviour
    {
        [SerializeField] private Button _spellButton;
        [SerializeField] private Image _image;
        [SerializeField] private Image _loadingVisual;

        [SerializeField] private Slider _slider;

        [SerializeField] private TextMeshProUGUI _numberOfUsesText;

        [SerializeField] private AudioClip _rechargedSound;
        [SerializeField] private AudioClip _deniedSound;
        [SerializeField] private AudioClip _dropSound;

        private Animator_SpellButton _animator;

        private AdjustOpacity _imageOpacity;

        private const float BUTTON_REMOVE_DURATION = 0.5f;
        private const float BUTTON_PRESS_DURATION = 0.2f;

        private bool _isRemoveOngoing = false;
        private bool _isNewSpellQued = false;

        private Coroutine _cooldownRoutine;

        #region INITIALIZE
        private void Awake()
        {
            _animator = GetComponent<Animator_SpellButton>();

            _imageOpacity = GetComponent<AdjustOpacity>();

            _spellButton.onClick.AddListener(ButtonPressed);

        }
        #endregion

        #region BUTTON ACTIONS
        private void ButtonPressed()
        {
            if (SpellManager.Instance.CastHeldSpell())
            {
                _animator.PlayCastAnimation();
            }
            else
            {
                AudioPlayer.Instance.PlaySoundEffect(_deniedSound);
                _animator.PlayDeniedAnimation();
            }
        }

        private void InitializeSpellButton()
        {
            if (_isRemoveOngoing)
            {
                _isNewSpellQued = true;
            }
            else
            {
                _isNewSpellQued = false;

                _image.sprite = SpellManager.Instance.GetSpriteOfHeldSpell();// set up the sprite in the UI

                _loadingVisual.sprite = _image.sprite;

                UpdateNumberOfUses(SpellManager.Instance.GetUsageNumberHeldSpell());

                _imageOpacity.StartTransition(0.1f, Ease.Unset, true);

                _animator.PlayActivateAnimation();
            }
        }

        private void RemoveButton(bool isBecauseOutOfUses)
        {
            if (_isRemoveOngoing)
            {
                Debug.LogWarning("Cannot remove spell because a spell is already being removed");
            }
            else
            {
                StartCoroutine(WaitForSpellRemoveVisuals(isBecauseOutOfUses));
            }
        }

        private IEnumerator WaitForSpellRemoveVisuals(bool isBecauseOutOfUses)
        {
            //If the spell is out of uses it has to wait for the button press animation
            //If not then the spell is dropped and the drop sound plays
            if (isBecauseOutOfUses)
            {
                yield return new WaitForSeconds(BUTTON_PRESS_DURATION);
            }
            else
            {
                AudioPlayer.Instance.PlaySoundEffect(_dropSound);
            }

            _isRemoveOngoing = true;

            //Reset cooldown
            if (_cooldownRoutine != null)
            {
                StopCoroutine(_cooldownRoutine);
                _cooldownRoutine = null;
            }
            _slider.value = 1;
            _spellButton.interactable = true;


            _imageOpacity.StartTransition();
            _animator.PlayRemoveAnimation();

            yield return new WaitForSeconds(BUTTON_REMOVE_DURATION);

            _isRemoveOngoing = false;

            if (_isNewSpellQued)
            {
                InitializeSpellButton();
            }
        }
        #endregion

        #region COOLDOWN

        private IEnumerator SpellCooldownRoutine(float cooldown)
        {
            yield return new WaitForSeconds(BUTTON_PRESS_DURATION);

            _spellButton.interactable = false;
            _slider.value = 0; // Start at empty

            float elapsedTime = 0f;

            while (elapsedTime < cooldown)
            {
                elapsedTime += TimeScaleManager.GetDeltaTimeScale();
                _slider.value = elapsedTime / cooldown; // Normalize between 0 and 1
                yield return null;
            }

            _slider.value = 1;
            _spellButton.interactable = true;

            AudioPlayer.Instance.PlaySoundEffect(_rechargedSound);
        }

        private void UpdateNumberOfUses(int numberOfUses)
        {
            _numberOfUsesText.text = numberOfUses.ToString();
        }

        #endregion

        #region EVENTS

        private void SpellWasCast(Spell spell, int remainingNumberOfUses, float cooldown)
        {
            _cooldownRoutine = StartCoroutine(SpellCooldownRoutine(cooldown));
            UpdateNumberOfUses(remainingNumberOfUses);
        }

        private void OnEnable()
        {
            SpellManager.OnSpellCast += SpellWasCast;
            SpellManager.OnRemoveSpell += RemoveButton;
            SpellManager.OnAddSpell += InitializeSpellButton;
        }

        private void OnDisable()
        {
            SpellManager.OnSpellCast -= SpellWasCast;
            SpellManager.OnRemoveSpell -= RemoveButton;
            SpellManager.OnAddSpell -= InitializeSpellButton;
        }
        #endregion
    }
}
