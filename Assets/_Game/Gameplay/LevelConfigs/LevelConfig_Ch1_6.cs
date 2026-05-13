using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第6关 — 暗元素使
    /// 
    /// 背景：掌握暗影本源力量的高级施法者，能操控暗影创造实体。
    /// 暗元素使每回合召唤「暗影之灵」协助攻击。
    ///
    /// 战斗设计：
    ///   - 3波次战斗
    ///   - 特殊机制「暗影召唤」：每回合结束召唤1个暗影之灵（HP=10, ATK=2）
    ///   - 暗影之灵存在时享受攻击力加成；击杀暗影之灵可获得能量回复
    ///
    /// 敌人：暗元素使 (Dark Elementalist)
    ///   - HP: 110
    ///   - 攻击方式: 每回合造成6点伤害
    ///   - 特性: 暗影召唤（每回合）
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_6", menuName = "CardMatch/LevelConfig/Chapter1-6")]
    public class LevelConfig_Ch1_6 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_006";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 6;

        [Header("敌人配置 — 暗元素使")]
        [SerializeField] private int     _enemyHp           = 110;
        [SerializeField] private int     _enemyAttack        = 6;
        [SerializeField] private int     _enemyShield        = 0;
        [SerializeField] private string  _enemyName          = "暗元素使";
        [SerializeField] private string  _enemyDescription  = "掌握暗影本源的高级施法者，能从虚空中召唤暗影之灵。";
        [SerializeField] private int     _summonHp           = 10;  // 暗影之灵HP
        [SerializeField] private int     _summonAttack       = 2;   // 暗影之灵攻击力
        [SerializeField] private int     _summonBonusPerSpirit = 1; // 每个暗影之灵使本体+1伤害

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 3;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId    = 6;
        [SerializeField] private float       _shadowElementRatio = 0.25f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 350;
        [SerializeField] private string[]   _cardRewards       = { "card_007" };

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
        public int     SummonHp           => _summonHp;
        public int     SummonAttack       => _summonAttack;
        public int     SummonBonusPerSpirit => _summonBonusPerSpirit;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     ShadowElementId     => _shadowElementId;
        public float   ShadowElementRatio  => _shadowElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;
    }
}
