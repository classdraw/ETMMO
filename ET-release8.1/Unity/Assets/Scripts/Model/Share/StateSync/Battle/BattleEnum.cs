namespace ET
{
    public enum SelectType
    {
        None=0,
        Self=1,//自身
        Single=2,//单一目标 参数配置阵营  客户端传入
        SelfFan=3,//自身扇形
        SelfRectangle=4,//自身矩形
        SelfFanRectangle=5,//自身扇形的直线矩形
        DstFan=6,//目标扇形
        DstRectangle=7,//目标矩形
        DstFanRectangle=8,//目标扇形的直线矩形
        Position=9,//坐标 客户端传入
    }

}

