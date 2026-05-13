using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第2关 — 暗影斥候
    /// 
    /// 背景：玩家追踪暗影猎手的踪迹，来到城镇边缘的废弃仓库。
    /// 暗影斥候是猎手的前哨，擅长投掷淬毒飞刀。
    ///
    /// 战斗设计：
    ///   - 2波次战斗，难度略高于第1关
    ///   - 特殊机制「中毒」：每回合受到持续伤害
    ///   - 引入元素：毒元素（ElementType.TypeG，绿），每消除5个造成1点额外中毒伤害
    ///
    /// 敌人：暗影斥候 (Shadow Scout)
    ///   - HP: 30
    ///   - 攻击方式: 每回合对玩家造成3点伤害 + 中毒（每回合额外2点，持续2回合）
    ///   - 特性: 中毒 · 每波次恢复一定HP
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_2", menuName = "CardMatch/LevelConfig/Chapter1-2")]
    public class LevelConfig_Ch1_2 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_002";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 2;

        [Header("敌人配置 — 暗影斥候")]
        [SerializeField] private int     _enemyHp          = 30;
        [SerializeField] private int      _enemyAttack       = 3;
        [SerializeField] private int      _enemyShield       = 0;
        [SerializeField] private string   _enemyName         = "暗影斥候";
        [SerializeField] private string   _enemyDescription = "暗影猎手的侦察兵，擅长投掷淬毒飞刀，令目标中毒。";
        [SerializeField] private int       _enemyPoisonDamage = 2;
        [SerializeField] private int       _enemyPoisonDuration = 2;

        [Header("波次配置")]
        [SerializeField] private int       _totalWaves        = 2;
        [SerializeField] private int       _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool      _elementBasedDamage = true;

        [Header("特殊元素 — 毒元素")]
        [SerializeField] private int        _poisonElementId   = 7;  // ElementType.TypeG (绿)
        [SerializeField] private float       _poisonElementRatio = 0.15f; // 15%毒元素
        [SerializeField] private int          _piecesPerPoisonTick = 5; // 每消除5个毒元素，中毒+1回合

        [Header("通关奖励")]
        [SerializeField] private int        _scoreReward       = 150;
        [SerializeField] private string[]   _cardRewards       = { "card_003" };

        [Header("回合限制")]
        [SerializeField] private int        _maxTurns          = 25;

        // ── Properties ─────────────────────────────────────────
        public string  LevelId            => _levelId;
        public string  ChapterId          => _chapterId;
        public int     LevelIndex         => _levelIndex;
        public string  LevelName          => $"Chapter{_chapterId}_Level{_levelIndex}";
        public string  EnemyName          => _enemyName;
        public int     EnemyHp            => _enemyHp;
        public int     EnemyAttack        => _enemyAttack;
        public int     EnemyShield        => _enemyShield;
        public string  EnemyDescription   => _enemyDescription;
        public int     EnemyPoisonDamage  => _enemyPoisonDamage;
        public int     EnemyPoisonDuration => _enemyPoisonDuration;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage     => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     PoisonElementId     => _poisonElementId;
        public float   PoisonElementRatio  => _poisonElementRatio;
        public int     PiecesPerPoisonTick => _piecesPerPoisonTick;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;

        public static LevelConfig_Ch1_2 CreateDefault()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig_Ch1_2>();
            config._levelId              = "ch1_002";
            config._chapterId            = "ch1";
            config._levelIndex           = 2;
            config._enemyHp              = 30;
            config._enemyAttack          = 3;
            config._enemyShield          = 0;
            config._enemyName            = "暗影斥候";
            config._enemyDescription     = "暗影猎手的侦察兵，擅长投掷淬毒飞刀，令目标中毒。";
            config._enemyPoisonDamage    = 2;
            config._enemyPoisonDuration  = 2;
            config._totalWaves           = 2;
            config._piecesPerWave        = 36;
            config._piecesPerDamage       = 3;
            config._elementBasedDamage    = true;
            config._poisonElementId       = 7;
            config._poisonElementRatio     = 0.15f;
            config._piecesPerPoisonTick    = 5;
            config._scoreReward           = 150;
            config._cardRewards           = new[] { "card_003" };
            config._maxTurns              = 25;
            return config;
        }
    }
}
