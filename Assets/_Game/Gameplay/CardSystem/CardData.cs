using System;
using UnityEngine;

namespace CardMatch.Gameplay.CardSystem
{
    /// <summary>
    /// 卡牌类型枚举
    /// </summary>
    public enum CardType
    {
        None        = 0,
        ClearRow    = 1,   // 清除一整行
        ClearCol    = 2,   // 清除一整列
        ClearArea   = 3,   // 3×3 区域清除
        Bomb        = 4,   // 消除指定类型的所有棋子
        Shuffle     = 5,   // 重置棋盘（随机打乱）
        Swap        = 6,   // 强制交换两个棋子
        EnergyBoost = 7,   // 直接给能量
        Heal        = 8    // 回血
    }

    /// <summary>
    /// 卡牌数据 ScriptableObject — 定义单张卡牌的元数据
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "CardMatch/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("卡牌唯一ID")]
        public string CardId;

        [Tooltip("卡牌显示名称")]
        public string CardName;

        [Tooltip("卡牌类型")]
        public ECardType CardType;

        [Tooltip("能耗点数")]
        public int EnergyCost;

        [Tooltip("卡牌描述文字")]
        [TextArea(2, 4)]
        public string Description;

        [Header("美术资源")]
        [Tooltip("卡牌图标 Sprite 路径（Resources 加载）")]
        public string IconPath;

        [Tooltip("稀有度颜色")]
        public Color RarityColor = Color.white;

        [Header("效果参数")]
        [Tooltip("效果范围半径（用于 ClearArea、Bomb）")]
        public int Range;

        [Tooltip("效果数值（如 EnergyBoost 给多少能量）")]
        public int Value;

        [Tooltip("指定元素类型（用于 Bomb）")]
        public ElementType TargetElement;
    }
}