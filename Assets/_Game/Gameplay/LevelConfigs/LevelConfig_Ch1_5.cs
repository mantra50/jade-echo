using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第5关 — 暗影骑士
    /// 
    /// 背景：城镇废墟的守卫者，暗影骑士身披黑曜石铠甲。
    /// 它的护盾能抵挡部分伤害，需要先用重击击碎。
    ///
    /// 战斗设计：
    ///   - 3波次战斗，引入护盾机制
    ///   - 特殊机制「护盾」：每波次开始时获得30点临时护盾
    ///   - 护盾存在时，受到的伤害降低50%
    ///
    /// 敌人：暗影骑士 (Shadow Knight)
    ///   - HP: 85
    ///   - 攻击方式: 每回合造成5点伤害
    ///   - 特性: 护盾壁垒（每波次重置）
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_5", menuName = "CardMatch/LevelConfig/Chapter1-5")]
    public class LevelConfig_Ch1_5 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_005";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 5;

        [Header("敌人配置 — 暗影骑士")]
        [SerializeField] private int     _enemyHp           = 85;
        [SerializeField] private int     _enemyAttack        = 5;
        [SerializeField] private int     _enemyShield        = 0;
        [SerializeField] private string  _enemyName          = "暗影骑士";
        [SerializeField] private string  _enemyDescription  = "黑曜石铠甲的亡灵骑士，护盾能抵挡一半伤害。";
        [SerializeField] private int     _waveShieldAmount   = 30; // 每波次护盾
        [SerializeField] private float   _damageReduction    = 0.5f; // 护盾下伤害减半

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 3;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _earthElementId    = 2;  // ElementType.TypeC (黄/土)
        [SerializeField] private float       _earthElementRatio = 0.20f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 300;
        [SerializeField] private string[]   _cardRewards       = { "card_006" };

        [Header("回合限制")]
        [SerializeField] private int          _maxTurns          = 20;

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
        public int     WaveShieldAmount   => _waveShieldAmount;
        public float   DamageReduction    => _damageReduction;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     EarthElementId      => _earthElementId;
        public float   EarthElementRatio   => _earthElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;
    }
}
