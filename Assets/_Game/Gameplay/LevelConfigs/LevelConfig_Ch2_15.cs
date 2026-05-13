using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter2 第15关 — 裂隙领航者
    /// 
    /// 背景：裂隙纪元的指挥官型敌人，能够为自身叠加护盾，
    /// 同时召唤裂隙仆从协助防守。是 Chapter2 混合机制的集大成者。
    ///
    /// 战斗设计：
    ///   - 3波次战斗
    ///   - 特殊机制「护盾+召唤（混合）」：敌人每回合开始时+3护盾；每4回合召唤1只仆从
    ///   - 护盾和召唤物共存，需要玩家同时应对两种机制
    ///   - 护盾上限为60点；召唤间隔为4回合
    ///
    /// 敌人：裂隙领航者 (Rift Navigator)
    ///   - HP: 190 / 护甲: 0 / 攻击: 14
    ///   - 被动护盾：每回合+3护盾，上限60
    ///   - 召唤：每4回合召唤HP=25/攻击=6的仆从
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch2_15", menuName = "CardMatch/LevelConfig/Chapter2-15")]
    public class LevelConfig_Ch2_15 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch2_015";
        [SerializeField] private string _chapterId = "ch2";
        [SerializeField] private int    _levelIndex = 15;

        [Header("敌人配置 — 裂隙领航者")]
        [SerializeField] private int     _enemyHp              = 190;
        [SerializeField] private int     _enemyAttack           = 14;
        [SerializeField] private int     _enemyArmor            = 0;
        [SerializeField] private string  _enemyName             = "裂隙领航者";
        [SerializeField] private string  _enemyDescription     = "裂隙纪元的指挥官，能为自身叠加护盾并召唤仆从协助作战。";
        [SerializeField] private int      _shieldPerTurn        = 3;
        [SerializeField] private int      _maxShield            = 60;
        [SerializeField] private int      _summonInterval       = 4;
        [SerializeField] private int      _summonHp             = 25;
        [SerializeField] private int      _summonAttack         = 6;
        [SerializeField] private int      _maxSummons           = 3;

        [Header("波次配置")]
        [SerializeField] private int       _totalWaves        = 3;
        [SerializeField] private int        _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int        _piecesPerDamage   = 3;
        [SerializeField] private bool       _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId   = 6;
        [SerializeField] private float        _shadowElementRatio = 0.25f;
        [SerializeField] private int         _voidElementId     = 8;
        [SerializeField] private float        _voidElementRatio  = 0.18f;
        [SerializeField] private int         _fireElementId     = 5;
        [SerializeField] private float        _fireElementRatio  = 0.12f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 500;
        [SerializeField] private string[]   _cardRewards       = { "card_022" };

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
        public int     ShieldPerTurn       => _shieldPerTurn;
        public int     MaxShield           => _maxShield;
        public int     SummonInterval      => _summonInterval;
        public int     SummonHp            => _summonHp;
        public int     SummonAttack        => _summonAttack;
        public int     MaxSummons          => _maxSummons;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave       => _piecesPerWave;
        public int     PiecesPerDamage     => _piecesPerDamage;
        public bool    ElementBasedDamage => _elementBasedDamage;
        public int     ShadowElementId     => _shadowElementId;
        public float   ShadowElementRatio => _shadowElementRatio;
        public int     VoidElementId      => _voidElementId;
        public float   VoidElementRatio   => _voidElementRatio;
        public int     FireElementId       => _fireElementId;
        public float   FireElementRatio   => _fireElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;

        public static LevelConfig_Ch2_15 CreateDefault()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig_Ch2_15>();
            config._levelId              = "ch2_015";
            config._chapterId            = "ch2";
            config._levelIndex           = 15;
            config._enemyHp              = 190;
            config._enemyAttack          = 14;
            config._enemyArmor           = 0;
            config._enemyName            = "裂隙领航者";
            config._enemyDescription     = "裂隙纪元的指挥官，能为自身叠加护盾并召唤仆从协助作战。";
            config._shieldPerTurn         = 3;
            config._maxShield            = 60;
            config._summonInterval       = 4;
            config._summonHp             = 25;
            config._summonAttack         = 6;
            config._maxSummons            = 3;
            config._totalWaves           = 3;
            config._piecesPerWave        = 36;
            config._piecesPerDamage       = 3;
            config._elementBasedDamage    = true;
            config._shadowElementId      = 6;
            config._shadowElementRatio   = 0.25f;
            config._voidElementId         = 8;
            config._voidElementRatio     = 0.18f;
            config._fireElementId        = 5;
            config._fireElementRatio     = 0.12f;
            config._scoreReward          = 500;
            config._cardRewards          = new[] { "card_022" };
            config._maxTurns             = 20;
            return config;
        }
    }
}