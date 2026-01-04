using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a Monster species' base data.
/// This is the template from which runtime Monster instances are created.
/// </summary>
[CreateAssetMenu(fileName = "New Monster", menuName = "SNES Christmas RPG/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Display name of the monster")]
    public string monsterName;

    [Tooltip("Short description of the monster")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("The SNES game this character originates from")]
    public string sourceGame;

    [Header("Sprites")]
    [Tooltip("Sprite shown in overworld/menus")]
    public Sprite portraitSprite;

    [Tooltip("Sprite shown during battle (player side)")]
    public Sprite battleSprite;

    [Tooltip("Sprite shown during battle (enemy side, typically flipped)")]
    public Sprite battleSpriteEnemy;

    [Tooltip("Small icon for UI elements")]
    public Sprite iconSprite;

    [Header("Battle Prefab & Animation")]
    [Tooltip("The prefab instantiated in battle (should have MonsterAnimator component)")]
    public GameObject battlePrefab;

    [Tooltip("The Animator Controller containing all animation states for this monster")]
    public RuntimeAnimatorController animatorController;

    [Tooltip("Custom technique animation mappings (technique name -> animator trigger)")]
    public List<TechniqueAnimationMapping> techniqueAnimations = new List<TechniqueAnimationMapping>();

    [Header("Base Stats (Level 1 Values)")]
    [Tooltip("LIFE - Determines maximum HP")]
    [Range(1, 999)]
    public int baseLife = 20;

    [Tooltip("POWER - Physical damage dealt/reduced")]
    [Range(1, 999)]
    public int basePower = 10;

    [Tooltip("INTELLIGENCE - Intelligence damage dealt/reduced, Guts regen")]
    [Range(1, 999)]
    public int baseIntelligence = 10;

    [Tooltip("SKILL - Accuracy and critical hit rate")]
    [Range(1, 999)]
    public int baseSkill = 10;

    [Tooltip("SPEED - ATB fill rate and evasion")]
    [Range(1, 999)]
    public int baseSpeed = 10;

    [Tooltip("DEFENSE - Reduces physical damage received")]
    [Range(1, 999)]
    public int baseDefense = 10;

    [Header("Growth Rankings (Determines stat gain per level)")]
    [Tooltip("S=+10-15, A=+8-10, B=+6-8, C=+4-6, D=+2-4, F=+0-1 per level")]
    public GrowthRank lifeGrowth = GrowthRank.C;
    public GrowthRank powerGrowth = GrowthRank.C;
    public GrowthRank intelligenceGrowth = GrowthRank.C;
    public GrowthRank skillGrowth = GrowthRank.C;
    public GrowthRank speedGrowth = GrowthRank.C;
    public GrowthRank defenseGrowth = GrowthRank.C;

    [Header("Techniques")]
    [Tooltip("Techniques learned at specific levels")]
    public List<LearnableTechnique> learnableTechniques = new List<LearnableTechnique>();

    [Header("Capture Settings")]
    [Tooltip("Base capture rate modifier (1.0 = normal, higher = easier to catch)")]
    [Range(0.1f, 3.0f)]
    public float captureRateModifier = 1.0f;

    [Header("Experience")]
    [Tooltip("Base XP awarded when defeated")]
    public int baseExperienceYield = 10;

    /// <summary>
    /// Gets the stat growth range for a given rank.
    /// Returns min and max values for random growth per level.
    /// </summary>
    public static (int min, int max) GetGrowthRange(GrowthRank rank)
    {
        return rank switch
        {
            GrowthRank.S => (10, 15),
            GrowthRank.A => (8, 10),
            GrowthRank.B => (6, 8),
            GrowthRank.C => (4, 6),
            GrowthRank.D => (2, 4),
            GrowthRank.F => (0, 1),
            _ => (4, 6) // Default to C rank
        };
    }

    /// <summary>
    /// Gets the growth rank for a specific stat type.
    /// </summary>
    public GrowthRank GetGrowthRank(StatType statType)
    {
        return statType switch
        {
            StatType.Life => lifeGrowth,
            StatType.Power => powerGrowth,
            StatType.Intelligence => intelligenceGrowth,
            StatType.Skill => skillGrowth,
            StatType.Speed => speedGrowth,
            StatType.Defense => defenseGrowth,
            _ => GrowthRank.C
        };
    }

    /// <summary>
    /// Gets the base stat value for a specific stat type.
    /// </summary>
    public int GetBaseStat(StatType statType)
    {
        return statType switch
        {
            StatType.Life => baseLife,
            StatType.Power => basePower,
            StatType.Intelligence => baseIntelligence,
            StatType.Skill => baseSkill,
            StatType.Speed => baseSpeed,
            StatType.Defense => baseDefense,
            _ => 10
        };
    }

    /// <summary>
    /// Gets all techniques that should be known at a given level.
    /// </summary>
    public List<TechniqueData> GetTechniquesAtLevel(int level)
    {
        List<TechniqueData> techniques = new List<TechniqueData>();
        foreach (var learnable in learnableTechniques)
        {
            if (learnable.levelLearned <= level && learnable.technique != null)
            {
                techniques.Add(learnable.technique);
            }
        }
        return techniques;
    }

    /// <summary>
    /// Gets the technique learned at exactly the specified level, if any.
    /// </summary>
    public TechniqueData GetTechniqueLearnedAtLevel(int level)
    {
        foreach (var learnable in learnableTechniques)
        {
            if (learnable.levelLearned == level && learnable.technique != null)
            {
                return learnable.technique;
            }
        }
        return null;
    }
}

/// <summary>
/// Growth rankings that determine stat increases per level.
/// Based on GDD specifications.
/// </summary>
public enum GrowthRank
{
    S,  // +10-15 per level (Exceptional)
    A,  // +8-10 per level (Excellent)
    B,  // +6-8 per level (Good)
    C,  // +4-6 per level (Average)
    D,  // +2-4 per level (Poor)
    F   // +0-1 per level (Terrible)
}

/// <summary>
/// The six primary stats for monsters.
/// </summary>
public enum StatType
{
    Life,
    Power,
    Intelligence,
    Skill,
    Speed,
    Defense
}

/// <summary>
/// Represents a technique that can be learned at a specific level.
/// </summary>
[Serializable]
public class LearnableTechnique
{
    [Tooltip("The level at which this technique is learned")]
    [Range(1, 50)]
    public int levelLearned = 1;

    [Tooltip("Reference to the technique data")]
    public TechniqueData technique;
}

/// <summary>
/// Maps a technique to a specific animation trigger name.
/// Allows different monsters to have different animations for the same technique type.
/// </summary>
[Serializable]
public class TechniqueAnimationMapping
{
    [Tooltip("The technique this mapping is for")]
    public TechniqueData technique;

    [Tooltip("The animator trigger name to play for this technique")]
    public string animatorTriggerName;

    [Tooltip("Override the technique's default animation duration")]
    public bool overrideDuration = false;

    [Tooltip("Custom duration for this monster's version of the technique")]
    public float customDuration = 1f;
}
