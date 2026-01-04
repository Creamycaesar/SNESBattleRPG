using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Handles animation playback for a Monster in battle.
/// Attach this to each Monster's battle prefab.
/// 
/// Standard Animation States (all monsters should have these):
/// - Idle: Default state, loops continuously
/// - Move: Walking/approaching animation (optional for battle)
/// - Damage: Played when taking a hit
/// - Dead: Played when HP reaches 0
/// - Victory: Played when battle is won
/// 
/// Technique animations are triggered by name, allowing each monster
/// to have unique attack animations that map to their techniques.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class MonsterAnimator : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation Settings")]
    [Tooltip("Time to wait before returning to Idle after a technique")]
    [SerializeField] private float returnToIdleDelay = 0.5f;

    // Standard animation parameter names (triggers)
    // These should match the trigger names in your Animator Controller
    public static class AnimParams
    {
        // State Triggers
        public const string Idle = "Idle";
        public const string Move = "Move";
        public const string Damage = "Damage";
        public const string Dead = "Dead";
        public const string Victory = "Victory";

        // Common Technique Triggers (monsters can have custom ones too)
        public const string Attack = "Attack";      // Basic attack
        public const string Technique = "Technique"; // Generic technique

        // Boolean for death state (stays dead)
        public const string IsDead = "IsDead";
    }

    // Events for battle system to hook into
    public event Action OnAnimationComplete;
    public event Action OnDamageFrameReached;  // For syncing damage application

    private Coroutine currentAnimationCoroutine;
    private bool isDead = false;

    private void Awake()
    {
        // Auto-get components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnValidate()
    {
        // Auto-populate in editor
        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Sets up the animator controller at runtime.
    /// Called by BattleManager when spawning monsters.
    /// </summary>
    public void Initialize(RuntimeAnimatorController controller)
    {
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        isDead = false;
        PlayIdle();
    }

    /// <summary>
    /// Plays the idle animation (default state).
    /// </summary>
    public void PlayIdle()
    {
        if (isDead) return;
        animator.SetTrigger(AnimParams.Idle);
    }

    /// <summary>
    /// Plays the movement animation.
    /// </summary>
    public void PlayMove()
    {
        if (isDead) return;
        animator.SetTrigger(AnimParams.Move);
    }

    /// <summary>
    /// Plays the damage/hit reaction animation.
    /// </summary>
    public void PlayDamage(Action onComplete = null)
    {
        if (isDead) return;

        StopCurrentAnimation();
        currentAnimationCoroutine = StartCoroutine(PlayAnimationAndReturn(
            AnimParams.Damage,
            returnToIdleDelay,
            onComplete
        ));
    }

    /// <summary>
    /// Plays the death animation and keeps the monster in dead state.
    /// </summary>
    public void PlayDead(Action onComplete = null)
    {
        isDead = true;
        animator.SetBool(AnimParams.IsDead, true);
        animator.SetTrigger(AnimParams.Dead);

        // Optionally fade out or hide after death animation
        if (onComplete != null)
        {
            StartCoroutine(InvokeAfterAnimation(AnimParams.Dead, onComplete));
        }
    }

    /// <summary>
    /// Plays the victory pose animation.
    /// </summary>
    public void PlayVictory()
    {
        if (isDead) return;
        animator.SetTrigger(AnimParams.Victory);
    }

    /// <summary>
    /// Plays a technique animation by trigger name.
    /// Use this for monster-specific technique animations.
    /// </summary>
    /// <param name="techniqueTriggerName">The trigger name in the Animator Controller</param>
    /// <param name="duration">How long before returning to idle</param>
    /// <param name="onComplete">Callback when animation finishes</param>
    public void PlayTechnique(string techniqueTriggerName, float duration, Action onComplete = null)
    {
        if (isDead) return;

        StopCurrentAnimation();

        // Check if the trigger exists, fall back to generic if not
        if (HasParameter(techniqueTriggerName))
        {
            currentAnimationCoroutine = StartCoroutine(PlayAnimationAndReturn(
                techniqueTriggerName,
                duration,
                onComplete
            ));
        }
        else
        {
            Debug.LogWarning($"Animation trigger '{techniqueTriggerName}' not found. Using generic Attack.");
            currentAnimationCoroutine = StartCoroutine(PlayAnimationAndReturn(
                AnimParams.Attack,
                duration,
                onComplete
            ));
        }
    }

    /// <summary>
    /// Plays a technique using TechniqueData's animation settings.
    /// </summary>
    public void PlayTechnique(TechniqueData technique, Action onComplete = null)
    {
        if (technique == null)
        {
            PlayTechnique(AnimParams.Attack, returnToIdleDelay, onComplete);
            return;
        }

        // Use the technique name as the trigger (formatted for animator)
        string triggerName = FormatTechniqueNameForAnimator(technique.techniqueName);
        PlayTechnique(triggerName, technique.animationDuration, onComplete);
    }

    /// <summary>
    /// Flashes the sprite (for damage feedback).
    /// </summary>
    public void FlashDamage(Color flashColor, float duration = 0.1f, int flashes = 3)
    {
        StartCoroutine(FlashRoutine(flashColor, duration, flashes));
    }

    /// <summary>
    /// Sets sprite facing direction (for battle positioning).
    /// </summary>
    public void SetFacing(bool facingRight)
    {
        spriteRenderer.flipX = !facingRight;
    }

    /// <summary>
    /// Resets the monster to alive state (for revive effects).
    /// </summary>
    public void Revive()
    {
        isDead = false;
        animator.SetBool(AnimParams.IsDead, false);
        spriteRenderer.color = Color.white;
        PlayIdle();
    }

    // ========== Helper Methods ==========

    private void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
    }

    private IEnumerator PlayAnimationAndReturn(string triggerName, float duration, Action onComplete)
    {
        animator.SetTrigger(triggerName);

        yield return new WaitForSeconds(duration);

        if (!isDead)
        {
            PlayIdle();
        }

        onComplete?.Invoke();
        OnAnimationComplete?.Invoke();
        currentAnimationCoroutine = null;
    }

    private IEnumerator InvokeAfterAnimation(string triggerName, Action callback)
    {
        // Wait for the animation to start
        yield return null;

        // Get the current animation length
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        callback?.Invoke();
    }

    private IEnumerator FlashRoutine(Color flashColor, float duration, int flashes)
    {
        Color originalColor = spriteRenderer.color;

        for (int i = 0; i < flashes; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(duration);
        }
    }

    private bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Converts a technique name to an animator trigger name.
    /// "Repulsor Blast" -> "RepulsorBlast"
    /// </summary>
    private string FormatTechniqueNameForAnimator(string techniqueName)
    {
        if (string.IsNullOrEmpty(techniqueName))
            return AnimParams.Attack;

        // Remove spaces and special characters
        return techniqueName.Replace(" ", "").Replace("-", "").Replace("'", "");
    }

    // ========== Animation Events ==========
    // These can be called from Animation Events in the Unity Editor

    /// <summary>
    /// Call this from an Animation Event at the damage frame of an attack.
    /// </summary>
    public void AnimEvent_DamageFrame()
    {
        OnDamageFrameReached?.Invoke();
    }

    /// <summary>
    /// Call this from an Animation Event when animation completes.
    /// </summary>
    public void AnimEvent_Complete()
    {
        OnAnimationComplete?.Invoke();
    }
}