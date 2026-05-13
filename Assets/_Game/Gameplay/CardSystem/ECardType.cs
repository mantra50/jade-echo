namespace CardMatch.Gameplay.CardSystem
{
    /// <summary>
    /// 卡牌类型枚举（供 UI 层使用，与 CardData.CardType 对应）
    /// </summary>
    public enum ECardType
    {
        None        = 0,
        Attack      = 1,   // 攻击类：直接造成伤害
        Defense     = 2,   // 防御类：生成护盾/减伤
        Transform   = 3,   // 转化类：改变棋盘棋子属性
        Utility     = 4    // 功能类：能量获取/抽卡/重排等
    }
}
