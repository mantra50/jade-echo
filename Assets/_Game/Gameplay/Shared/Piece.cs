namespace CardMatch.Gameplay
{
    /// <summary>
    /// 棋盘元素类型（6 种宝石 + 无）
    /// </summary>
    public enum ElementType
    {
        None = 0,
        TypeA = 1,   // 红
        TypeB = 2,   // 橙
        TypeC = 3,   // 黄
        TypeD = 4,   // 绿
        TypeE = 5,   // 蓝
        TypeF = 6    // 紫
    }

    /// <summary>
    /// 棋子状态机
    /// </summary>
    public enum PieceState
    {
        Idle       = 0,   // 静止
        Selected   = 1,   // 选中（玩家拖拽）
        Swapping   = 2,   // 交换动画中
        Eliminating = 3,  // 即将消除
        Falling    = 4    // 下落中
    }

    /// <summary>
    /// 棋盘棋子数据模型（值对象，供 MatchSystem 使用）
    /// </summary>
    public class Piece
    {
        public ElementType Type   { get; set; }
        public PieceState  State  { get; set; }
        public int         X      { get; set; }
        public int         Y      { get; set; }
        public bool        IsNew  { get; internal set; }  // 是否为本次下落新生成

        public override string ToString() =>
            $"Piece({Type}, [{X},{Y}], {State})";
    }
}