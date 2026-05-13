using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter2 第12关 — 裂隙游猎者
    /// 
    /// 背景：裂隙纪元的游猎者，穿行于裂隙之间搜寻猎物。
    /// 它们会在战斗中召唤裂隙仆从协助作战，形成以多敌少的局面。
    ///
    /// 战斗设计：
    ///   - 3波次战斗（每波次召唤1只裂隙仆从）
    ///   - 特殊机制「召唤流」：敌人每3回合召唤一只裂隙仆从（HP=20，攻击=5）
    ///   - 裂隙仆从会被玩家的消除伤害AOE波及
    ///   - 击杀仆从不获得额外奖励，但可减少敌人总输出
    ///
    /// 敌人：裂隙游猎者 (Rift Stalker)
    ///   - HP: 100 / 护甲: 0 / 攻击: 10
    ///   - 召唤间隔: 3回合 / 召唤物HP: 20 / 召唤物攻击: 5
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch2_12", menuName = "CardMatch/LevelConfig/Chapter2-12")]
    public class LevelConfig_Ch2_12 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch2_012";
        [SerializeField] private string _chapterId = "ch2";
        [SerializeField] private int    _levelIndex = 12;

        [Header("敌人配置 — 裂隙游猎者")]
        [SerializeField] private int     _enemyHp              = 100;
        [SerializeField] private int     _enemyAttack           = 10;
        [SerializeField] private int     _enemyArmor            = 0;
        [SerializeField] private string  _enemyName             = "裂隙游猎者";
        [SerializeField] private string  _enemyDescription     = "穿行于裂隙间的猎手，每隔数回合便会召唤仆从协同作战。";
        [SerializeField] private int      _summonInterval       = 3;
        [SerializeField] private int      _summonHp             = 20;
        [SerializeField] private int      _summonAttack         = 5;
        [SerializeField] private int      _maxSummons           = 4;

        [Header("波次配置")]
        [SerializeField] private int       _totalWaves        = 3;
        [SerializeField] private int        _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int        _piecesPerDamage   = 3;
        [SerializeField] private bool       _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId   = 6;
        [SerializeField] private float        _shadowElementRatio = 0.28f;
        [SerializeField] private int         _fireElementId     = 5;
        [SerializeField] private float        _fireElementRatio  = 0.12f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 300;
        [SerializeField] private string[]   _cardRewards       = { "card_015" };

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
        public int     EnemyArmor         => _enemyArmor;
        public string  EnemyDescription   => _enemyDescription;
        public int     SummonInterval     => _summonInterval;
        public int     SummonHp           => _summonHp;
        public int     SummonAttack       => _summonAttack;
        public int     MaxSummons         => _maxSummons;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     ShadowElementId     => _shadowElementId;
        public float   ShadowElementRatio => _shadowElementRatio;
        public int     FireElementId       => _fireElementId;
        public float   FireElementRatio    => _fireElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;

        public static LevelConfig_Ch2_12 CreateDefault()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig_Ch2_12>();
            config._levelId              = "ch2_012";
            config._chapterId            = "ch2";
            config._levelIndex           = 12;
            config._enemyHp              = 100;
            config._enemyAttack          = 10;
            config._enemyArmor           = 0;
            config._enemyName            = "裂隙游猎者";
            config._enemyDescription     = "穿行于裂隙间的猎手，每隔数回合便会召唤仆从协同作战。";
            config._summonInterval        = 3;
            config._summonHp              = 20;
            config._summonAttack          = 5;
            config._maxSummons            = 4;
            config._totalWaves           = 3;
            config._piecesPerWave        = 36;
            config._piecesPerDamage       = 3;
            config._elementBasedDamage    = true;
            config._shadowElementId       = 6;
            config._shadowElementRatio    = 0.28f;
            config._fireElementId         = 5;
            config._fireElementRatio       = 0.12f;
            config._scoreReward           = 300;
            config._cardRewards           = new[] { "card_015" };
            config._maxTurns              = 20;
            return config;
        }
    }
}