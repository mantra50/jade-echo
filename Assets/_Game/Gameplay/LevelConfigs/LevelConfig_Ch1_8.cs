using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter1 第8关 — 暗影指挥官
    /// 
    /// 背景：统领所有暗影军团的精英将领，能激励周围的暗影单位。
    /// 指挥官的存在使场上所有暗影单位获得攻击力加成。
    ///
    /// 战斗设计：
    ///   - 3波次战斗，Boss+小怪混合
    ///   - 特殊机制「指挥光环」：场上每有1个暗影单位，指挥官+2攻击力
    ///   - 波次之间召唤2个暗影士兵（HP=20, ATK=3）助战
    ///
    /// 敌人：暗影指挥官 (Shadow Commander)
    ///   - HP: 200
    ///   - 攻击方式: 每回合造成7点基础伤害 + 指挥光环加成
    ///   - 特性: 指挥光环（召唤小怪）
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_8", menuName = "CardMatch/LevelConfig/Chapter1-8")]
    public class LevelConfig_Ch1_8 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_008";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 8;

        [Header("敌人配置 — 暗影指挥官")]
        [SerializeField] private int     _enemyHp           = 200;
        [SerializeField] private int     _enemyAttack        = 7;
        [SerializeField] private int     _enemyShield        = 0;
        [SerializeField] private string  _enemyName          = "暗影指挥官";
        [SerializeField] private string  _enemyDescription  = "暗影军团的高阶将领，能激励周围的暗影单位协同作战。";
        [SerializeField] private int     _commandBonusPerUnit = 2; // 每个暗影单位+2攻击
        [SerializeField] private int     _minionCountPerWave = 2;  // 每波召唤小怪数
        [SerializeField] private int     _minionHp           = 20;
        [SerializeField] private int     _minionAttack       = 3;

        [Header("波次配置")]
        [SerializeField] private int      _totalWaves        = 3;
        [SerializeField] private int      _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int       _piecesPerDamage   = 3;
        [SerializeField] private bool     _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _multiElementRatio = 0.28f; // 多元素混合

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 500;
        [SerializeField] private string[]   _cardRewards       = { "card_009", "card_010" };

        [Header("回合限制")]
        [SerializeField] private int          _maxTurns          = 16;

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
        public int     CommandBonusPerUnit => _commandBonusPerUnit;
        public int     MinionCountPerWave  => _minionCountPerWave;
        public int     MinionHp           => _minionHp;
        public int     MinionAttack       => _minionAttack;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public float   MultiElementRatio   => _multiElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;
    }
}
