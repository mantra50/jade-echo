using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第7关 — 夜袭者
    /// 
    /// 背景：擅长夜战和暗杀的精英暗影单位，战斗中会进入隐身状态。
    /// 隐身期间无法被锁定（普攻消除伤害-50%），现身后造成暴击。
    ///
    /// 战斗设计：
    ///   - 3波次战斗
    ///   - 特殊机制「隐身」：每3回合进入隐身（1回合），隐身期间受伤减半
    ///   - 隐身结束现身的第1次攻击必定暴击（2倍伤害）
    ///
    /// 敌人：夜袭者 (Nightstalker)
    ///   - HP: 150
    ///   - 攻击方式: 普通6伤，暴击12伤
    ///   - 特性: 隐身（3回合周期，隐身时受伤-50%）· 现身暴击
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_7", menuName = "CardMatch/LevelConfig/Chapter1-7")]
    public class LevelConfig_Ch1_7 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_007";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 7;

        [Header("敌人配置 — 夜袭者")]
        [SerializeField] private int     _enemyHp           = 150;
        [SerializeField] private int     _enemyAttack        = 6;
        [SerializeField] private int     _enemyShield        = 0;
        [SerializeField] private string  _enemyName          = "夜袭者";
        [SerializeField] private string  _enemyDescription  = "精英暗杀单位，擅长隐身突袭，现身必暴击。";
        [SerializeField] private int     _stealthCycle       = 3; // 每3回合隐身1回合
        [SerializeField] private float   _stealthDamageReduction = 0.5f;
        [SerializeField] private int     _critMultiplier     = 2; // 暴击倍率

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 3;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _darkElementId     = 6;
        [SerializeField] private float       _darkElementRatio = 0.25f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 400;
        [SerializeField] private string[]   _cardRewards       = { "card_008" };

        [Header("回合限制")]
        [SerializeField] private int          _maxTurns          = 18;

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
        public int     StealthCycle       => _stealthCycle;
        public float   StealthDamageReduction => _stealthDamageReduction;
        public int     CritMultiplier     => _critMultiplier;
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
