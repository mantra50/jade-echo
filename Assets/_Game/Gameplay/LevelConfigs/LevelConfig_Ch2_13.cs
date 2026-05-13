using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter2 第13关 — 暴击裂隙兽
    /// 
    /// 背景：被裂隙能量完全改造的野兽，攻击变得异常凶猛。
    /// 它的利爪会在关键时刻爆发出致命一击，令玩家防不胜防。
    ///
    /// 战斗设计：
    ///   - 2波次战斗
    ///   - 特殊机制「暴击流」：敌人20%几率造成双倍伤害（暴击）
    ///   - 暴击时伤害显示特殊特效，提醒玩家
    ///   - 每波次敌人攻击递增（每波次+2基础伤害）
    ///
    /// 敌人：暴击裂隙兽 (Rift Beast)
    ///   - HP: 130 / 护甲: 0 / 攻击: 12
    ///   - 暴击率: 20% / 暴击倍率: 2.0x
    ///   - 每波次+2攻击（递增）
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch2_13", menuName = "CardMatch/LevelConfig/Chapter2-13")]
    public class LevelConfig_Ch2_13 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch2_013";
        [SerializeField] private string _chapterId = "ch2";
        [SerializeField] private int    _levelIndex = 13;

        [Header("敌人配置 — 暴击裂隙兽")]
        [SerializeField] private int     _enemyHp              = 130;
        [SerializeField] private int     _enemyAttack           = 12;
        [SerializeField] private int     _enemyArmor            = 0;
        [SerializeField] private string  _enemyName             = "暴击裂隙兽";
        [SerializeField] private string  _enemyDescription     = "被裂隙能量改造的野兽，利爪可在关键时刻爆发出致命一击。";
        [SerializeField] private float    _critChance           = 0.20f;
        [SerializeField] private float    _critMultiplier      = 2.0f;
        [SerializeField] private int     _attackIncreasePerWave = 2;

        [Header("波次配置")]
        [SerializeField] private int       _totalWaves        = 2;
        [SerializeField] private int        _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int        _piecesPerDamage   = 3;
        [SerializeField] private bool       _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId   = 6;
        [SerializeField] private float        _shadowElementRatio = 0.32f;
        [SerializeField] private int         _fireElementId     = 5;
        [SerializeField] private float        _fireElementRatio  = 0.15f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 350;
        [SerializeField] private string[]   _cardRewards       = { "card_019" };

        [Header("回合限制")]
        [SerializeField] private int          _maxTurns          = 20;

        // ── Properties ─────────────────────────────────────────
        public string  LevelId             => _levelId;
        public string  ChapterId           => _chapterId;
        public int     LevelIndex          => _levelIndex;
        public string  LevelName          => $"Chapter{_chapterId}_Level{_levelIndex}";
        public string  EnemyName          => _enemyName;
        public int     EnemyHp            => _enemyHp;
        public int     EnemyAttack        => _enemyAttack;
        public int     EnemyArmor         => _enemyArmor;
        public string  EnemyDescription   => _enemyDescription;
        public float   CritChance          => _critChance;
        public float   CritMultiplier     => _critMultiplier;
        public int     AttackIncreasePerWave => _attackIncreasePerWave;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage => _elementBasedDamage;
        public int     ShadowElementId    => _shadowElementId;
        public float   ShadowElementRatio => _shadowElementRatio;
        public int     FireElementId      => _fireElementId;
        public float   FireElementRatio   => _fireElementRatio;
        public int     ScoreReward        => _scoreReward;
        public string[] CardRewards       => _cardRewards;
        public int     MaxTurns           => _maxTurns;

        public static LevelConfig_Ch2_13 CreateDefault()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig_Ch2_13>();
            config._levelId              = "ch2_013";
            config._chapterId            = "ch2";
            config._levelIndex           = 13;
            config._enemyHp              = 130;
            config._enemyAttack          = 12;
            config._enemyArmor           = 0;
            config._enemyName            = "暴击裂隙兽";
            config._enemyDescription     = "被裂隙能量改造的野兽，利爪可在关键时刻爆发出致命一击。";
            config._critChance            = 0.20f;
            config._critMultiplier       = 2.0f;
            config._attackIncreasePerWave = 2;
            config._totalWaves           = 2;
            config._piecesPerWave        = 36;
            config._piecesPerDamage       = 3;
            config._elementBasedDamage    = true;
            config._shadowElementId       = 6;
            config._shadowElementRatio    = 0.32f;
            config._fireElementId         = 5;
            config._fireElementRatio      = 0.15f;
            config._scoreReward           = 350;
            config._cardRewards           = new[] { "card_019" };
            config._maxTurns              = 20;
            return config;
        }
    }
}