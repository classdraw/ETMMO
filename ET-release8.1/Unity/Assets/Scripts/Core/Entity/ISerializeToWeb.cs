namespace ET
{
    /**
      ISerializeToWeb 的作用是：标记「服务端运行时子 Entity，序列化父对象时要带上」。
它和 ISerializeToEntity 在框架里走同一套逻辑，区别主要是语义——区分「玩家存档数据」和「服务端状态 / Web 后台可查看的数据」。
没挂这两个接口的子 Entity 默认不会随父 Entity 一起序列化。
服务端运行时状态，供 Web/后台查看
     */
    public interface ISerializeToWeb
    {
    }
}