using System;
using UnityEngine;

namespace CardMatch.Gameplay
{
    /// <summary>
    /// Chapter2 第11关 — 裂隙幽灵
    /// 
    /// 背景：裂隙纪元的低等暗影实体，飘荡在裂隙边缘的能量场中。
    /// 它们被能量渗透后获得了「护盾叠加」特性——每次受到攻击都会积累新的护盾层。
    ///
    /// 战斗设计：
    ///   - 2波次战斗
    ///   - 特殊机制「护盾叠加」：敌人每次受到攻击（玩家消除伤害），在受到伤害的同时积累新护盾
    ///   - 护盾每5层提供5点护盾值，可叠加；护盾可吸收伤害
    ///   - 护盾满时将护盾转化为额外攻击
    ///
    /// 敌人：裂隙幽灵 (Rift Phantom)
    ///   - HP: 80 / 护甲: 0 / 攻击: 8
    ///   - 护盾叠加规则：每受到5次攻击，+5护盾（可叠加）
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_Ch2_11", menuName = "CardMatch/LevelConfig/Chapter2-11")]
    public class LevelConfig_Ch2_11 : ScriptableObject
    {
        [Header("关卡信息")]
        [SerializeField] private string _levelId   = "ch2_011";
        [SerializeField] private string _chapterId = "ch2";
        [SerializeField] private int    _levelIndex = 11;

        [Header("敌人配置 — 裂隙幽灵")]
        [SerializeField] private int     _enemyHp              = 80;
        [SerializeField] private int     _enemyAttack           = 8;
        [SerializeField] private int     _enemyArmor            = 0;
        [SerializeField] private string   _enemyName             = "裂隙幽灵";
        [SerializeField] private string   _enemyDescription     = "被裂隙能量渗透的幽魂，攻击会在它身上积累护盾层。";
        [SerializeField] private int      _shieldHitsPerLayer   = 5;  // 每受击5次叠加一层护盾
        [SerializeField] private int      _shieldPerLayer       = 5;  // 每层护盾提供5点护盾值
        [SerializeField] private int      _maxShieldLayers      = 6;  // 最多叠加6层

        [Header("波次配置")]
        [SerializeField] private int       _totalWaves        = 2;
        [SerializeField] private int        _piecesPerWave     = 36;

        [Header("伤害配置")]
        [SerializeField] private int        _piecesPerDamage   = 3;
        [SerializeField] private bool       _elementBasedDamage = true;

        [Header("特殊元素")]
        [SerializeField] private int        _shadowElementId   = 6;
        [SerializeField] private float        _shadowElementRatio = 0.30f;
        [SerializeField] private int         _voidElementId     = 8;
        [SerializeField] private float        _voidElementRatio  = 0.10f;

        [Header("通关奖励")]
        [SerializeField] private int         _scoreReward       = 250;
        [SerializeField] private string[]   _cardRewards       = { "card_014" };

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
        public int     ShieldHitsPerLayer => _shieldHitsPerLayer;
        public int     ShieldPerLayer     => _shieldPerLayer;
        public int     MaxShieldLayers    => _maxShieldLayers;
        public int     TotalWaves         => _totalWaves;
        public int     PiecesPerWave      => _piecesPerWave;
        public int     PiecesPerDamage    => _piecesPerDamage;
        public bool    ElementBasedDamage  => _elementBasedDamage;
        public int     ShadowElementId     => _shadowElementId;
        public float   ShadowElementRatio => _shadowElementRatio;
        public int     VoidElementId       => _voidElementId;
        public float   VoidElementRatio    => _voidElementRatio;
        public int     ScoreReward         => _scoreReward;
        public string[] CardRewards        => _cardRewards;
        public int     MaxTurns            => _maxTurns;

        public static LevelConfig_Ch2_11 CreateDefault()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig_Ch2_11>();
            config._levelId              = "ch2_011";
            config._chapterId            = "ch2";
            config._levelIndex           = 11;
            config._enemyHp              = 80;
            config._enemyAttack          = 8;
            config._enemyArmor           = 0;
            config._enemyName            = "裂隙幽灵";
            config._enemyDescription     = "被裂隙能量渗透的幽魂，攻击会在它身上积累护盾层。";
            config._shieldHitsPerLayer    = 5;
            config._shieldPerLayer       = 5;
            config._maxShieldLayers       = 6;
            config._totalWaves           = 2;
            config._piecesPerWave        = 36;
            config._piecesPerDamage       = 3;
            config._elementBasedDamage    = true;
            config._shadowElementId       = 6;
            config._shadowElementRatio    = 0.30f;
            config._voidElementId         = 8;
            config._voidElementRatio      = 0.10f;
            config._scoreReward           = 250;
            config._cardRewards           = new[] { "card_014" };
            config._maxTurns               = 20;
            return config;
        }
    }
}