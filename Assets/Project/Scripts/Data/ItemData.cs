using UnityEngine;
using System;

/// <summary>
/// ScriptableObject defining an Item's data.
/// Items in this game are exclusively grown from the farming system
/// and used outside of battle to permanently boost Monster stats or grant XP.
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "SNES Christmas RPG/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Display name of the item")]
    public string itemName;

    [Tooltip("Description shown in inventory")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Icon displayed in inventory and menus")]
    public Sprite icon;

    [Header("Item Type")]
    [Tooltip("What kind of boost this item provides")]
    public FarmItemType itemType = FarmItemType.StatBoost;

    [Header("Stat Boost Settings")]
    [Tooltip("Which stat this item boosts (ignored for XP boost items)")]
    public StatType targetStat = StatType.Life;

    [Tooltip("Immediate stat points added when used")]
    [Range(0, 100)]
    public int statBoostAmount = 50;

    [Tooltip("Whether this item also improves the Monster's growth ranking for the stat")]
    public bool improvesGrowthRanking = true;

    [Header("XP Boost Settings")]
    [Tooltip("Amount of XP granted when used (only for XP boost items)")]
    [Range(0, 10000)]
    public int xpBoostAmount = 500;

    [Header("Visual Feedback")]
    [Tooltip("Color used for the stat increase text popup")]
    public Color feedbackColor = Color.green;

    [Tooltip("Sound effect played when item is used")]
    public AudioClip useSoundEffect;

    /// <summary>
    /// Returns true if this item boosts a stat.
    /// </summary>
    public bool IsStatBoostItem()
    {
        return itemType == FarmItemType.StatBoost;
    }

    /// <summary>
    /// Returns true if this item grants XP.
    /// </summary>
    public bool IsXPBoostItem()
    {
        return itemType == FarmItemType.XPBoost;
    }

    /// <summary>
    /// Gets the message displayed when this item is used on a Monster.
    /// </summary>
    public string GetUseMessage(string monsterName)
    {
        if (itemType == FarmItemType.XPBoost)
        {
            return $"{monsterName} gained {xpBoostAmount} experience points!";
        }

        string statName = GetStatDisplayName(targetStat);

        if (improvesGrowthRanking)
        {
            return $"{monsterName}'s {statName} increased! It seems to have greater potential now...";
        }

        return $"{monsterName}'s {statName} increased by {statBoostAmount}!";
    }

    /// <summary>
    /// Gets the display name for a stat type.
    /// </summary>
    public static string GetStatDisplayName(StatType stat)
    {
        return stat switch
        {
            StatType.Life => "LIFE",
            StatType.Power => "POWER",
            StatType.Intelligence => "INTELLIGENCE",
            StatType.Skill => "SKILL",
            StatType.Speed => "SPEED",
            StatType.Defense => "DEFENSE",
            _ => stat.ToString()
        };
    }

    /// <summary>
    /// Gets the abbreviated display name for a stat type.
    /// </summary>
    public static string GetStatAbbreviation(StatType stat)
    {
        return stat switch
        {
            StatType.Life => "LIF",
            StatType.Power => "POW",
            StatType.Intelligence => "INT",
            StatType.Skill => "SKI",
            StatType.Speed => "SPD",
            StatType.Defense => "DEF",
            _ => "???"
        };
    }

    /// <summary>
    /// Gets a formatted description including the item's effects.
    /// </summary>
    public string GetFormattedDescription()
    {
        if (itemType == FarmItemType.XPBoost)
        {
            return $"{description}\n\nGrants {xpBoostAmount} XP to a Monster.";
        }

        string statName = GetStatDisplayName(targetStat);
        string desc = $"{description}\n\n+{statBoostAmount} {statName}";

        if (improvesGrowthRanking)
        {
            desc += $"\nImproves {statName} growth potential by one rank.";
        }

        return desc;
    }
}

/// <summary>
/// Types of items that can be grown from the farming system.
/// </summary>
public enum FarmItemType
{
    [Tooltip("Boosts a specific stat and improves growth ranking")]
    StatBoost,

    [Tooltip("Grants immediate XP to a Monster")]
    XPBoost
}