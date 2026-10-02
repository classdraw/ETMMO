using System;

namespace ET
{
    public class AIHandlerAttribute: BaseAttribute
    {
    }
    
    [AIHandler]
    public abstract class AAIHandler: HandlerObject
    {
        // 检查是否满足条件（定时调度，用于切换 Current 节点）
        public abstract int Check(AIComponent aiComponent, AIConfig aiConfig);

        /// <summary>Current 指向本节点时每帧 Update；返回 0 保持节点，非 0 结束本节点。</summary>
        public abstract int Update(AIComponent aiComponent, AIConfig aiConfig);
    }
}