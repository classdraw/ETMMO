namespace ET.Server
{
    /// <summary>占位：后续按怪物普攻逻辑扩展。AIConfigId=1 可引用。</summary>
    public class AI_Attack : AAIHandler
    {
        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            return aiComponent.GetParent<Unit>() == null ? 1 : 1;
        }

        public override int Update(AIComponent aiComponent, AIConfig aiConfig)
        {
            return 1;
        }
    }
}
