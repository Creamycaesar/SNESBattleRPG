using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a Technique's data.
/// Techniques are special moves that Monsters can learn and use in battle.
/// </summary>
[CreateAssetMenu(fileName = "New Technique", menuName = "SNES Christmas RPG/Technique Data")]
public class TechniqueData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Display name of the technique")]
    public string techniqueName;

    [Tooltip("Description shown in menus")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Icon displayed in technique menus")]
    public Sprite icon;

    [Header("Technique Classification")]
    [Tooltip("The category of this technique")]
    public TechniqueCategory category = TechniqueCategory.Physical;

    [Tooltip("What this technique targets")]
    public TargetType targetType = TargetType.SingleEnemy;

    [Header("Cost & Usage")]
    [Tooltip("Guts cost to use this technique (0 for basic attacks)")]
    [Range(0, 100)]
    public int gutsCost = 10;

    [Tooltip("Number of times this technique can be used per battle (-1 for unlimited)")]
    public int usesPerBattle = -1;

    [Header("Power & Accuracy")]
    [Tooltip("Base power for damage calculation (higher = more damage)")]
    [Range(0, 300)]
    public int power = 50;

    [Tooltip("Base accuracy percentage (100 = guaranteed hit before modifiers)")]
    [Range(0, 100)]
    public int baseAccuracy = 95;

    [Tooltip("Critical hit rate bonus (added to SKI/20 calculation)")]
    [Range(0, 50)]
    public int criticalBonus = 0;

    [Tooltip("Number of hits this technique performs")]
    [Range(1, 10)]
    public int hitCount = 1;

    [Header("Support Effects")]
    [Tooltip("For healing/support techniques - base heal amount or buff strength")]
    [Range(0, 999)]
    public int effectValue = 0;

    [Tooltip("For buff/debuff techniques - which stat is affected")]
    public StatType affectedStat = StatType.Life;

    [Tooltip("Duration in turns for buff/debuff effects (0 = instant/permanent)")]
    [Range(0, 10)]
    public int effectDuration = 0;

    [Tooltip("Percentage modifier for buffs/debuffs (e.g., 25 = +25% or -25%)")]
    [Range(-100, 100)]
    public int effectPercentage = 0;

    [Header("Status Effects")]
    [Tooltip("Status effect this technique may inflict")]
    public StatusEffect statusEffect = StatusEffect.None;

    [Tooltip("Chance to inflict the status effect (0-100%)")]
    [Range(0, 100)]
    public int statusChance = 0;

    [Header("Special Properties")]
    [Tooltip("Ignores target's defense stats")]
    public bool ignoresDefense = false;

    [Tooltip("Always hits (ignores accuracy/evasion)")]
    public bool alwaysHits = false;

    [Tooltip("Can critical hit")]
    public bool canCritical = true;

    [Tooltip("Makes contact with target (relevant for certain effects)")]
    public bool makesContact = true;

    [Tooltip("Affects user instead of/in addition to target")]
    public bool affectsSelf = false;

    [Tooltip("Priority modifier (-1 to +1, higher goes first within same ATB)")]
    [Range(-1, 1)]
    public int priority = 0;

    [Header("Animation & Effects")]
    [Tooltip("Animation clip played when this technique is used")]
    public AnimationClip userAnimationClip;

    [Tooltip("Animation clip played on the target when hit")]
    public AnimationClip targetAnimationClip;

    [Tooltip("Particle effect prefab spawned on the user")]
    public GameObject userEffectPrefab;

    [Tooltip("Particle effect prefab spawned on the target")]
    public GameObject targetEffectPrefab;

    [Tooltip("Delay in seconds before damage is applied (sync with animation)")]
    [Range(0f, 3f)]
    public float damageDelay = 0.5f;

    [Tooltip("Total duration of the technique animation")]
    [Range(0.1f, 5f)]
    public float animationDuration = 1f;

    [Header("Audio")]
    [Tooltip("Sound effect played when technique is initiated")]
    public AudioClip useSoundEffect;

    [Tooltip("Sound effect played on impact/hit")]
    public AudioClip impactSoundEffect;

    [Header("Visual Customization")]
    [Tooltip("Color tint for damage numbers")]
    public Color damageColor = Color.white;

    [Tooltip("Screen shake intensity on hit (0 = none)")]
    [Range(0f, 1f)]
    public float screenShakeIntensity = 0.1f;

    /// <summary>
    /// Returns true if this technique deals damage.
    /// </summary>
    public bool IsDamagingTechnique()
    {
        return category == TechniqueCategory.Physical ||
               category == TechniqueCategory.Intelligence;
    }

    /// <summary>
    /// Returns true if this technique is a support/healing technique.
    /// </summary>
    public bool IsSupportTechnique()
    {
        return category == TechniqueCategory.Healing ||
               category == TechniqueCategory.Buff ||
               category == TechniqueCategory.Debuff;
    }

    /// <summary>
    /// Returns true if this technique targets enemies.
    /// </summary>
    public bool TargetsEnemies()
    {
        return targetType == TargetType.SingleEnemy ||
               targetType == TargetType.AllEnemies ||
               targetType == TargetType.RandomEnemy;
    }

    /// <summary>
    /// Returns true if this technique targets allies.
    /// </summary>
    public bool TargetsAllies()
    {
        return targetType == TargetType.SingleAlly ||
               targetType == TargetType.AllAllies ||
               targetType == TargetType.Self;
    }

    /// <summary>
    /// Returns true if this technique can target multiple entities.
    /// </summary>
    public bool IsMultiTarget()
    {
        return targetType == TargetType.AllEnemies ||
               targetType == TargetType.AllAllies ||
               targetType == TargetType.Everyone;
    }

    /// <summary>
    /// Calculates the actual Guts cost after any modifiers.
    /// Can be extended to include buffs/debuffs that affect cost.
    /// </summary>
    public int GetAdjustedGutsCost(float costModifier = 1f)
    {
        return Mathf.Max(0, Mathf.RoundToInt(gutsCost * costModifier));
    }

    /// <summary>
    /// Gets a formatted string describing the technique for UI display.
    /// </summary>
    public string GetFormattedDescription()
    {
        string desc = description;

        if (IsDamagingTechnique())
        {
            desc += $"\nPower: {power}";
        }

        if (category == TechniqueCategory.Healing)
        {
            desc += $"\nHeals: {effectValue} HP";
        }

        if (category == TechniqueCategory.Buff || category == TechniqueCategory.Debuff)
        {
            string sign = effectPercentage >= 0 ? "+" : "";
            desc += $"\n{affectedStat}: {sign}{effectPercentage}% for {effectDuration} turns";
        }

        if (statusEffect != StatusEffect.None && statusChance > 0)
        {
            desc += $"\n{statusChance}% chance to inflict {statusEffect}";
        }

        desc += $"\nGuts: {gutsCost}";

        return desc;
    }
}

/// <summary>
/// Categories of techniques that determine damage calculation.
/// </summary>
public enum TechniqueCategory
{
    [Tooltip("Damage scales with POWER stat")]
    Physical,

    [Tooltip("Damage scales with INTELLIGENCE stat")]
    Intelligence,

    [Tooltip("Restores HP to target")]
    Healing,

    [Tooltip("Increases target's stats temporarily")]
    Buff,

    [Tooltip("Decreases target's stats temporarily")]
    Debuff,

    [Tooltip("Special techniques with unique effects")]
    Special
}

/// <summary>
/// Defines what a technique can target.
/// </summary>
public enum TargetType
{
    [Tooltip("Targets one enemy, player chooses")]
    SingleEnemy,

    [Tooltip("Targets all enemies at once")]
    AllEnemies,

    [Tooltip("Targets a random enemy")]
    RandomEnemy,

    [Tooltip("Targets one ally, player chooses")]
    SingleAlly,

    [Tooltip("Targets all allies at once")]
    AllAllies,

    [Tooltip("Targets the user only")]
    Self,

    [Tooltip("Targets everyone on the field")]
    Everyone
}

/// <summary>
/// Status effects that can be inflicted by techniques.
/// </summary>
public enum StatusEffect
{
    [Tooltip("No status effect")]
    None,

    [Tooltip("Takes damage each turn")]
    Poison,

    [Tooltip("Takes damage each turn (stronger than poison)")]
    Burn,

    [Tooltip("Cannot act for a number of turns")]
    Stun,

    [Tooltip("Reduced accuracy")]
    Blind,

    [Tooltip("Reduced speed")]
    Slow,

    [Tooltip("Cannot use techniques (can still Attack)")]
    Silence,

    [Tooltip("May fail to act on turn")]
    Confusion,

    [Tooltip("Falls asleep, wakes when hit")]
    Sleep,

    [Tooltip("Cannot move or act")]
    Freeze,

    [Tooltip("Defense reduced significantly")]
    ArmorBreak,

    [Tooltip("Attack power reduced significantly")]
    WeakenedAttack
}