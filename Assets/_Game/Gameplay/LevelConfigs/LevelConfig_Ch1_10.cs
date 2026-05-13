using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第10关 — 蜃楼蛛母（最终Boss）
    /// 
    /// 背景：蛛母盘踞在虚空中最大的裂隙处，编织着笼罩一切的噩梦之网。
    /// 它能用蛛丝标记目标，叠满5层后将玩家定身，无法消除棋子。
    ///
    /// 战斗设计：
    ///   - 3波次最终Boss战，使用 BossSpider.cs
    ///   - 特殊机制「蛛网标记」：每次攻击给玩家附加1层标记（阶段二+2层）
    ///   - 叠满5层触发「定身」（无法消除，持续2回合）
    ///   - 每4回合召唤「蜃楼幻象」协助战斗
    ///   - 阶段二（HP≤20%）：终焉时刻，蛛网标记+2层/次，幻象加速
    ///
    /// 敌人：蜃楼蛛母 (Phantom Spider Queen)
    ///   - HP: 350 / 护甲: 15 / 攻击: 22
    ///   - 阶段二: HP≤70 时触发，伤害+30%，每攻击+2层标记
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_10", menuName = "CardMatch/LevelConfig/Chapter1-10")]
    public class LevelConfig_Ch1_10 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_010";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 10;
        [SerializeField] private bool    _isBossLevel = true;

        [Header("敌人配置 — 蜃楼蛛母")]
        [SerializeField] private int     _enemyHp              = 350;
        [SerializeField] private int     _enemyAttack           = 22;
        [SerializeField] private int     _enemyArmor            = 15;
        [SerializeField] private string  _enemyName             = "蜃楼蛛母";
        [SerializeField] private string  _enemyDescription     = "盘踞虚空最大裂隙的噩梦之主，以蛛丝编织的罗网困杀猎物。";
        [SerializeField] private int     _webMarkMax           = 5;
        [SerializeField] private int     _webDamagePerStack    = 3; // 每层蛛网每回合伤害
        [SerializeField] private int     _paralyzeDuration     = 2;  // 定身回合数
        [SerializeField] private int     _summonInterval       = 4;
        [SerializeField] private int     _illusionHp           = 20;
        [SerializeField] private int     _illusionAttack       = 10;
        [SerializeField] private float   _phaseTwoMultiplier   = 1.3f;

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 3;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId    = 6;
        [SerializeField] private float       _shadowElementRatio = 0.35f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 1000;
        [SerializeField] private string[]   _cardRewards       = { "card_012", "card_013" };

        [Header("回合限制")]
        [SerializeField] private int          _maxTurns          = 20;

        // ── Properties ─────────────────────────────────────────
        public string  LevelId            => _levelId;
        public string  ChapterId          => _chapterId;
        public int     LevelIndex          => _levelIndex;
        public string  LevelName          => $"Chapter{_chapterId}_Level{_levelIndex}";
        public bool    IsBossLevel         => _isBossLevel;
        public string  EnemyName          => _enemyName;
        public int     EnemyHp            => _enemyHp;
        public int     EnemyAttack        => _enemyAttack;
        public int     EnemyArmor         => _enemyArmor;
        public string  EnemyDescription   => _enemyDescription;
        public int     WebMarkMax         => _webMarkMax;
        public int     WebDamagePerStack  => _webDamagePerStack;
        public int     ParalyzeDuration   => _paralyzeDuration;
        public int     SummonInterval     => _summonInterval;
        public int     IllusionHp         => _illusionHp;
        public int     IllusionAttack     => _illusionAttack;
        public float   PhaseTwoMultiplier => _phaseTwoMultiplier;
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
