using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第3关 — 暗影弓手
    /// 
    /// 背景：穿过森林时遭遇远程敌人。
    /// 暗影弓手从阴影中射击，需要快速接近才能打断其连射。
    ///
    /// 战斗设计：
    ///   - 2波次战斗
    ///   - 特殊机制「蓄力射击」：蓄力2回合后造成高伤害（可被消除打断）
    ///   - 特殊元素：风元素，影响消除连锁
    ///
    /// 敌人：暗影弓手 (Shadow Archer)
    ///   - HP: 50
    ///   - 攻击方式: 普通射击3伤；蓄力满后「穿心箭」造成8点伤害
    ///   - 特性: 蓄力射击 · 蓄力期间被消除3次则打断
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_3", menuName = "CardMatch/LevelConfig/Chapter1-3")]
    public class LevelConfig_Ch1_3 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_003";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 3;

        [Header("敌人配置 — 暗影弓手")]
        [SerializeField] private int     _enemyHp              = 50;
        [SerializeField] private int     _enemyAttack           = 3;
        [SerializeField] private int     _enemyShield           = 0;
        [SerializeField] private string  _enemyName             = "暗影弓手";
        [SerializeField] private string  _enemyDescription     = "隐匿于阴影中的远程杀手，蓄力后的穿心箭威力惊人。";
        [SerializeField] private int     _enemyChargedAttack    = 8;
        [SerializeField] private int     _enemyChargeTurns      = 2;
        [SerializeField] private int     _interruptThreshold    = 3; // 蓄力期间消除3次则打断

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 2;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int      _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素 — 风元素")]
        [SerializeField] private int       _windElementId     = 5;  // ElementType.TypeE (蓝)
        [SerializeField] private float      _windElementRatio = 0.18f;

        [Header("通关奖励")]
        [SerializeField] private int       _scoreReward       = 200;
        [SerializeField] private string[]  _cardRewards       = { "card_004" };

        [Header("回合限制")]
        [SerializeField] private int        _maxTurns          = 25;

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
        public int     EnemyChargedAttack => _enemyChargedAttack;
        public int     EnemyChargeTurns    => _enemyChargeTurns;
        public int     InterruptThreshold  => _interruptThreshold;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     WindElementId       => _windElementId;
        public float   WindElementRatio    => _windElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;
    }
}
