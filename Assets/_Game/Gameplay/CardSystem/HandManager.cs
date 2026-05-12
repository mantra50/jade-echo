using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardMatch.Gameplay.CardSystem
{
    /// <summary>
    /// 手牌管理 — 负责抽卡、手牌展示、卡牌选中、能量消耗
    /// </summary>
    public class HandManager
    {
        private readonly List<CardData> _hand = new List<CardData>();
        private readonly int _maxHandSize;
        private int _currentEnergy;

        public event Action<int>            OnEnergyChanged;
        public event Action<CardData>       OnCardAdded;
        public event Action<CardData>       OnCardRemoved;
        public event Action<CardData>       OnCardSelected;
        public event Action                 OnHandUpdated;

        public IReadOnlyList<CardData> Hand       => _hand;
        public int                     CurEnergy  => _currentEnergy;
        public int                     MaxHandSize => _maxHandSize;

        public HandManager(int maxHandSize = 5)
        {
            _maxHandSize = maxHandSize;
        }

        /// <summary>设置当前可用能量</summary>
        public void SetEnergy(int energy)
        {
            _currentEnergy = Mathf.Max(0, energy);
            OnEnergyChanged?.Invoke(_currentEnergy);
        }

        /// <summary>抽一张卡（从牌组 SO 取，未实现抽牌逻辑时用随机卡代替）</summary>
        public bool TryDrawCard()
        {
            if (_hand.Count >= _maxHandSize) return false;

            // TODO: 从 DeckManager 抽取真实卡牌
            // 这里用随机测试卡代替
            var testCards = CreateTestCardPool();
            var card = testCards[UnityEngine.Random.Range(0, testCards.Count)];
            _hand.Add(card);
            OnCardAdded?.Invoke(card);
            OnHandUpdated?.Invoke();
            return true;
        }

        /// <summary>尝试打出手牌中指定索引的卡（检测能耗）</summary>
        public CardData TryPlayCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= _hand.Count) return null;

            var card = _hand[handIndex];
            if (card.EnergyCost > _currentEnergy) return null;

            // 扣能量，移出手牌
            _currentEnergy -= card.EnergyCost;
            OnEnergyChanged?.Invoke(_currentEnergy);
            _hand.RemoveAt(handIndex);
            OnCardRemoved?.Invoke(card);
            OnHandUpdated?.Invoke();
            return card;
        }

        /// <summary>选中手牌中指定索引的卡（UI 反馈用）</summary>
        public void SelectCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= _hand.Count) return;
            OnCardSelected?.Invoke(_hand[handIndex]);
        }

        /// <summary>清空全部手牌（每局结束后调用）</summary>
        public void ClearHand()
        {
            _hand.Clear();
            OnHandUpdated?.Invoke();
        }

        /// <summary>某卡牌是否可打出（能耗够）</summary>
        public bool CanPlay(CardData card) =>
            card != null && card.EnergyCost <= _currentEnergy;

        /// <summary>刷新能量（每回合重置用）</summary>
        public void RefillEnergy(int amount)
        {
            _currentEnergy = amount;
            OnEnergyChanged?.Invoke(_currentEnergy);
        }

        // ============================================================
        // 测试用卡池（正式版替换为 DeckManager）
        // ============================================================
        private List<CardData> CreateTestCardPool()
        {
            var pool = new List<CardData>();

            // 创建一个测试卡牌（不用 SO，直接 new）
            var clearRow = ScriptableObject.CreateInstance<CardData>();
            clearRow.CardId   = "clear_row_1";
            clearRow.CardName = "横斩";
            clearRow.CardType = CardType.ClearRow;
            clearRow.EnergyCost = 3;
            clearRow.Description = "清除棋盘上任意一行";
            clearRow.Range = 1;
            clearRow.IconPath = "Cards/icon_clear_row";
            clearRow.RarityColor = new Color(0.2f, 0.8f, 1f);

            var bomb = ScriptableObject.CreateInstance<CardData>();
            bomb.CardId   = "bomb_fire_1";
            bomb.CardName = "元素爆弹";
            bomb.CardType = CardType.Bomb;
            bomb.EnergyCost = 4;
            bomb.Description = "消除场上所有指定元素的棋子";
            bomb.Range = 0;
            bomb.Value = 0;
            bomb.TargetElement = ElementType.TypeA;
            bomb.IconPath = "Cards/icon_bomb";
            bomb.RarityColor = new Color(1f, 0.4f, 0.2f);

            var clearArea = ScriptableObject.CreateInstance<CardData>();
            clearArea.CardId   = "clear_area_1";
            clearArea.CardName = "星爆";
            clearArea.CardType = CardType.ClearArea;
            clearArea.EnergyCost = 5;
            clearArea.Description = "3×3 区域清除";
            clearArea.Range = 1;  // 1 = 3×3（含中心）
            clearArea.IconPath = "Cards/icon_clear_area";
            clearArea.RarityColor = new Color(0.8f, 0.2f, 1f);

            var energyBoost = ScriptableObject.CreateInstance<CardData>();
            energyBoost.CardId   = "energy_boost_1";
            energyBoost.CardName = "能量灌注";
            energyBoost.CardType = CardType.EnergyBoost;
            energyBoost.EnergyCost = 2;
            energyBoost.Description = "回复 20 能量点";
            energyBoost.Value = 20;
            energyBoost.IconPath = "Cards/icon_energy";
            energyBoost.RarityColor = new Color(1f, 0.9f, 0.2f);

            pool.Add(clearRow);
            pool.Add(bomb);
            pool.Add(clearArea);
            pool.Add(energyBoost);

            return pool;
        }
    }
}