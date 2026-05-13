using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第4关 — 暗影法师
    /// 
    /// 背景：森林深处的废弃祭坛，暗影法师正在施展禁忌仪式。
    /// 它能窃取玩家的能量，使卡牌无法使用。
    ///
    /// 战斗设计：
    ///   - 2波次战斗
    ///   - 特殊机制「能量窃取」：每回合偷走玩家2点能量
    ///   - 特殊元素：暗元素，用于放大技能效果
    ///
    /// 敌人：暗影法师 (Shadow Mage)
    ///   - HP: 65
    ///   - 攻击方式: 每回合造成4点伤害 + 偷取2点能量
    ///   - 特性: 能量窃取 · 暗元素增强
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_4", menuName = "CardMatch/LevelConfig/Chapter1-4")]
    public class LevelConfig_Ch1_4 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_004";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 4;

        [Header("敌人配置 — 暗影法师")]
        [SerializeField] private int     _enemyHp          = 65;
        [SerializeField] private int     _enemyAttack       = 4;
        [SerializeField] private int     _enemyShield       = 0;
        [SerializeField] private string  _enemyName         = "暗影法师";
        [SerializeField] private string  _enemyDescription = "精通暗影魔法的施法者，能窃取目标的能量供给己用。";
        [SerializeField] private int     _energyDrainAmount = 2;
        [SerializeField] private int     _darkEnhanceBonus  = 2; // 每有1个暗影元素，下次技能+2伤害

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 2;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素 — 暗元素")]
        [SerializeField] private int       _darkElementId     = 6;  // ElementType.TypeF (紫)
        [SerializeField] private float      _darkElementRatio = 0.22f;

        [Header("通关奖励")]
        [SerializeField] private int        _scoreReward       = 250;
        [SerializeField] private string[]  _cardRewards       = { "card_005" };

        [Header("回合限制")]
        [SerializeField] private int        _maxTurns          = 22;

        // ── Properties ─────────────────────────────────────────
        public string  LevelId            => _levelId;
        public string  ChapterId          => _chapterId;
        public int     LevelIndex          => _levelIndex;
        public string  LevelName          => $"Chapter{_chapterId}_Level{_levelIndex}";
        public string  EnemyName          => _enemyName;
        public int     EnemyHp            => _enemyHp;
        public int     EnemyAttack        => _enemyAttack;
        public int     EnemyShield        => _enemyShield;
        public string  EnemyDescription   => _enemyDescription;
        public int     EnergyDrainAmount  => _energyDrainAmount;
        public int     DarkEnhanceBonus   => _darkEnhanceBonus;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     DarkElementId       => _darkElementId;
        public float   DarkElementRatio    => _darkElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;
    }
}
