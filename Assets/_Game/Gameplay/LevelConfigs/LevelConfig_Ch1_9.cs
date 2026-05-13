using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第9关 — 裂隙守卫（准Boss）
    /// 
    /// 战斗设计：
    ///   - 3波次准Boss战
    ///   - 特殊机制「冻结格」：随机冻结棋盘2格（阶段二4格），持续2回合
    ///   - 每4回合召唤「裂隙影」助战（HP=15, ATK=4）
    ///   - 阶段二（HP≤50%）：狂暴，伤害+30%，冻结4格
    ///
    /// 敌人：裂隙守卫 (Rift Guardian) — 使用 BossGuardian.cs
    ///   - HP: 200 / 护甲: 30 / 攻击: 25
    ///   - 阶段二: HP≤100 时触发，攻击+30%，沙漏加速
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_9", menuName = "CardMatch/LevelConfig/Chapter1-9")]
    public class LevelConfig_Ch1_9 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_009";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 9;
        [SerializeField] private bool    _isBossLevel = true;

        [Header("敌人配置 — 裂隙守卫")]
        [SerializeField] private int     _enemyHp             = 200;
        [SerializeField] private int     _enemyAttack          = 25;
        [SerializeField] private int     _enemyArmor           = 30;
        [SerializeField] private string  _enemyName            = "裂隙守卫";
        [SerializeField] private string  _enemyDescription    = "虚空裂隙的守护者，能冻结棋盘格子阻止消除。";
        [SerializeField] private int     _freezeCellsPhase1   = 2;
        [SerializeField] private int     _freezeCellsPhase2   = 4;
        [SerializeField] private int     _freezeDuration      = 2;
        [SerializeField] private int     _summonInterval      = 4;
        [SerializeField] private int     _riftShadowHp        = 15;
        [SerializeField] private int     _riftShadowAttack    = 4;
        [SerializeField] private float   _phaseTwoMultiplier  = 1.3f;

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 3;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId    = 6;
        [SerializeField] private float       _shadowElementRatio = 0.30f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 600;
        [SerializeField] private string[]   _cardRewards       = { "card_011" };

        [Header("回合限制")]
        [SerializeField] private int          _maxTurns          = 15;

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
        public int     FreezeCellsPhase1  => _freezeCellsPhase1;
        public int     FreezeCellsPhase2  => _freezeCellsPhase2;
        public int     FreezeDuration     => _freezeDuration;
        public int     SummonInterval     => _summonInterval;
        public int     RiftShadowHp       => _riftShadowHp;
        public int     RiftShadowAttack   => _riftShadowAttack;
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
