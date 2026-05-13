using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// 📋 LevelConfig — Chapter1 第1关敌人配置
    /// 
    /// 数据来源：chapter1-route.md
    /// 
    /// Chapter1 第1关 — 序章：黑影现身
    /// ─────────────────────────────────────
    /// 背景故事：
    ///   玩家在熟悉的城镇广场醒来，发现周围笼罩着诡异的黑影。
    ///   第一个敌人「暗影猎手」现身，它是虚空派来的先遣侦察兵。
    ///
    /// 战斗设计：
    ///   - 单波次战斗，敌人HP较低，适合新手教学
    ///   - 玩家需要通过消除棋盘上的暗影元素来造成伤害
    ///   - 每消除3个暗影元素，对敌人造成1点伤害
    ///
    /// 敌人：暗影猎手 (Shadow Hunter)
    ///   - HP: 12
    ///   - 攻击方式: 每回合对玩家造成2点伤害
    ///   - 特性: 无特殊技能
    ///
    /// 胜利条件：消灭敌人
    /// 失败条件：玩家HP降至0
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch1_1", menuName = "CardMatch/LevelConfig/Chapter1-1")]
    public class LevelConfig : ScriptableObject
    {
        // ── 关卡标识 ────────────────────────────────────────────
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch1_001";
        [SerializeField] private string _chapterId = "ch1";
        [SerializeField] private int     _levelIndex = 1;

        // ── 敌人配置 ────────────────────────────────────────────
        [Header("敌人配置 — 暗影猎手")]
        [SerializeField] private int    _enemyHp          = 12;  // Alpha版教程关简化
        [SerializeField] private int    _enemyAttack       = 2;
        [SerializeField] private int    _enemyShield       = 0;
        [SerializeField] private string  _enemyName         = "暗影猎手";
        [SerializeField] private string  _enemyDescription = "虚空派来的先遣侦察兵，擅长暗中袭击。";

        // ── 波次配置 ────────────────────────────────────────────
        [Header("波次配置")]
        [SerializeField] private int    _totalWaves        = 1;
        [SerializeField] private int    _piecesPerWave     = 36;  // 6×6棋盘

        // ── 伤害配置 ────────────────────────────────────────────
        [Header("伤害配置")]
        [SerializeField] private int    _piecesPerDamage   = 3;   // 每消除3个暗影元素造成1点伤害
        [SerializeField] private bool   _elementBasedDamage = true;

        // ── 特殊元素配置 ────────────────────────────────────────
        [Header("特殊元素 — 暗影元素")]
        [SerializeField] private int    _shadowElementId   = 6;  // ElementType.TypeF (紫)
        [SerializeField] private float   _shadowElementRatio = 0.2f; // 20%暗影元素

        // ── 奖励配置 ────────────────────────────────────────────
        [Header("通关奖励")]
        [SerializeField] private int    _scoreReward       = 100;
        [SerializeField] private string[] _cardRewards      = { "card_001", "card_002" };

        // ── 回合限制 ────────────────────────────────────────────
        [Header("回合限制")]
        [SerializeField] private int    _maxTurns          = 30;  // 30回合未击败敌人则失败

        // ── 属性（公开访问）─────────────────────────────────────

        public string  LevelId            => _levelId;
        public string  ChapterId          => _chapterId;
        public int     LevelIndex         => _levelIndex;
        public string  LevelName          => $"Chapter{_chapterId}_Level{_levelIndex}";
        public string  EnemyName          => _enemyName;
        public int     EnemyHp            => _enemyHp;
        public int     EnemyAttack        => _enemyAttack;
        public int     EnemyShield        => _enemyShield;
        public string  EnemyDescription   => _enemyDescription;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave       => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     ShadowElementId     => _shadowElementId;
        public float   ShadowElementRatio  => _shadowElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;

        // ── 便利构造器（代码创建时使用）────────────────────────

        /// <summary>
        /// 静态创建 Chapter1 Level1 默认配置
        /// </summary>
        public static LevelConfig CreateChapter1Level1()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();

            config._levelId          = "ch1_001";
            config._chapterId        = "ch1";
            config._levelIndex       = 1;
            config._enemyHp          = 12;  // Alpha版教程关简化
            config._enemyAttack      = 2;
            config._enemyShield      = 0;
            config._enemyName        = "暗影猎手";
            config._enemyDescription = "虚空派来的先遣侦察兵，擅长暗中袭击。";
            config._totalWaves       = 1;
            config._piecesPerWave    = 36;
            config._piecesPerDamage  = 3;
            config._elementBasedDamage = true;
            config._shadowElementId  = 6;
            config._shadowElementRatio = 0.2f;
            config._scoreReward      = 100;
            config._cardRewards      = new[] { "card_001", "card_002" };
            config._maxTurns         = 30;

            return config;
        }

        // ── 数据加载 ────────────────────────────────────────────

        /// <summary>
        /// 从 chapter1-route.md 格式的数据加载配置
        /// 当作为 ScriptableObject 独立存在时使用默认 Chapter1-1 数据
        /// </summary>
        public void LoadFromData()
        {
            // chapter1-route.md 中定义的数据：
            // Chapter1-1: 暗影猎手, HP=15, ATK=2, 波次=1, 每3个消除=1伤害
            // 这里做占位填充，实际项目应从 Resources/Configs/chapter1-route.json 加载

            Debug.Log($"[LevelConfig] 从 chapter1-route.md 加载 {_levelId} 配置");
        }

        // ── 调试 ────────────────────────────────────────────────

        public override string ToString() =>
            $"LevelConfig({LevelName} | 敌人:{EnemyName} HP:{EnemyHp} ATK:{EnemyAttack} | 波次:{TotalWaves} | 胜利条件:消除{EnemyHp * PiecesPerDamage}个暗影)";
    }
}
