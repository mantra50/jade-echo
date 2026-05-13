using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter2 第14关 — 裂隙巨兽
    /// 
    /// 背景：裂隙纪元的顶级掠食者，体型巨大，与召唤出的裂隙幼体共享生命。
    /// 当幼体存活时，巨兽受到的部分伤害会转移到幼体上；当幼体死亡时，巨兽陷入狂暴。
    ///
    /// 战斗设计：
    ///   - 3波次战斗
    ///   - 特殊机制「HP共享」：敌人召唤2只裂隙幼体（HP=30），与幼体共享生命
    ///   - 幼体死亡时，巨兽获得「狂暴」：攻击力+50%，持续2回合
    ///   - 优先击杀幼体可触发狂暴但减少总体输出，击杀巨兽则幼体同步死亡
    ///
    /// 敌人：裂隙巨兽 (Rift Behemoth)
    ///   - HP: 160 / 护甲: 5 / 攻击: 15
    ///   - 召唤2只幼体，幼体HP: 30，幼体攻击: 8
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch2_14", menuName = "CardMatch/LevelConfig/Chapter2-14")]
    public class LevelConfig_Ch2_14 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch2_014";
        [SerializeField] private string _chapterId = "ch2";
        [SerializeField] private int    _levelIndex = 14;

        [Header("敌人配置 — 裂隙巨兽")]
        [SerializeField] private int     _enemyHp              = 160;
        [SerializeField] private int     _enemyAttack           = 15;
        [SerializeField] private int     _enemyArmor            = 5;
        [SerializeField] private string  _enemyName             = "裂隙巨兽";
        [SerializeField] private string  _enemyDescription     = "裂隙纪元的顶级掠食者，与召唤的幼体共享生命。";
        [SerializeField] private int      _minionCount          = 2;
        [SerializeField] private int      _minionHp             = 30;
        [SerializeField] private int      _minionAttack         = 8;
        [SerializeField] private float    _berserkAttackMultiplier = 1.5f;
        [SerializeField] private int      _berserkDuration      = 2;

        [Header("波次配置")]
        [SerializeField] private int       _totalWaves        = 3;
        [SerializeField] private int        _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int        _piecesPerDamage   = 3;
        [SerializeField] private bool       _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId   = 6;
        [SerializeField] private float        _shadowElementRatio = 0.30f;
        [SerializeField] private int         _voidElementId     = 8;
        [SerializeField] private float        _voidElementRatio  = 0.15f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 400;
        [SerializeField] private string[]   _cardRewards       = { "card_020" };

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
        public int     MinionCount         => _minionCount;
        public int     MinionHp            => _minionHp;
        public int     MinionAttack        => _minionAttack;
        public float   BerserkAttackMultiplier => _berserkAttackMultiplier;
        public int     BerserkDuration    => _berserkDuration;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage => _elementBasedDamage;
        public int     ShadowElementId    => _shadowElementId;
        public float   ShadowElementRatio => _shadowElementRatio;
        public int     VoidElementId      => _voidElementId;
        public float   VoidElementRatio   => _voidElementRatio;
        public int     ScoreReward        => _scoreReward;
        public string[] CardRewards       => _cardRewards;
        public int     MaxTurns           => _maxTurns;

        public static LevelConfig_Ch2_14 CreateDefault()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig_Ch2_14>();
            config._levelId              = "ch2_014";
            config._chapterId            = "ch2";
            config._levelIndex           = 14;
            config._enemyHp              = 160;
            config._enemyAttack          = 15;
            config._enemyArmor           = 5;
            config._enemyName            = "裂隙巨兽";
            config._enemyDescription     = "裂隙纪元的顶级掠食者，与召唤的幼体共享生命。";
            config._minionCount           = 2;
            config._minionHp              = 30;
            config._minionAttack          = 8;
            config._berserkAttackMultiplier = 1.5f;
            config._berserkDuration       = 2;
            config._totalWaves           = 3;
            config._piecesPerWave        = 36;
            config._piecesPerDamage       = 3;
            config._elementBasedDamage    = true;
            config._shadowElementId       = 6;
            config._shadowElementRatio    = 0.30f;
            config._voidElementId         = 8;
            config._voidElementRatio      = 0.15f;
            config._scoreReward           = 400;
            config._cardRewards           = new[] { "card_020" };
            config._maxTurns              = 20;
            return config;
        }
    }
}