
using UnityEngine;
using XEngine.Utilities;


namespace XEngine.Hud{
    public class HudMesh
    {
        public static float s_fCameraScale = 0.8f; // [0.1, 0.8]
        public static float s_fCameraScaleX = 0.8f;
        public static float s_fCameraScaleY = 0.8f;
        public static float s_fNumberScale = 1.0f;


        public Mesh m_kMesh;
        public Material m_kMaterial;
        public UIFont m_kFont;

        public BetterList<Vector3> m_kVerts=new BetterList<Vector3>();
        public BetterList<Vector2> m_kOffsets=new BetterList<Vector2>();
        public BetterList<Vector2> m_kUVs=new BetterList<Vector2>();
        public BetterList<Color32> m_kColors=new BetterList<Color32>();
        public BetterList<int> m_kIndices=new BetterList<int>();

        public float m_fScale=1.0f;
        int m_iOldSpriteNumb=0;
        CharacterInfo m_tempCharInfo;

        BetterList<HudVertex> m_kSpriteVertexs=new BetterList<HudVertex>();

        bool m_bQueryTexture=false;
        int m_iAtlasId;
        bool m_bDirty=false;
        bool m_bHaveNullVertex=false;
        public int AtlasId{
            get{
                return m_iAtlasId;
            }
            private set{
                m_iAtlasId=value;
            }
        }



        private static Shader GetFontShader()
        {
            HudBoardSetting setting = HudBoardSetting.GetInstance();
            if (setting.m_kFontShader != null)
            {
                return setting.m_kFontShader;
            }

            if (setting.m_kNumberShader != null)
            {
                return setting.m_kNumberShader;
            }

            return Shader.Find("Unlit/HUDFont");
        }

        private static Shader GetSpriteShader()
        {
            HudBoardSetting setting = HudBoardSetting.GetInstance();
            if (setting.m_kSpriteShader != null)
            {
                return setting.m_kSpriteShader;
            }

            if (setting.m_kNumberShader != null)
            {
                return setting.m_kNumberShader;
            }

            return Shader.Find("Unlit/HUDSprite");
        }

        public void SetAtlasId(int atlasId, bool useNumberShader = false){
            if(AtlasId!=atlasId&&AtlasId!=0){
                //旧的卸载
                ReleaseTexture();
            }
            AtlasId=atlasId;
            if (m_kMaterial == null)
            {
                // 图集/跳字：纹理颜色 * 顶点色
                m_kMaterial = new Material(GetSpriteShader());
                /* m_kMaterial =new Material(Shader.Find("Unlit/HUDSprite"));
                 if(m_kMaterial != null && Application.platform != RuntimePlatform.WindowsEditor)
                 {
                     m_kMaterial.EnableKeyword("MAIN_ALPHA_ON");
                 }*/
            }

            if(AtlasId!=0){
                QueryTexture();
            }
        }

        private void QueryTexture(){
            if(!m_bQueryTexture&&AtlasId!=0){
                m_bQueryTexture=true;
                OnLoadHudAtlas();
            }
        }
        
        float  GetReserveY()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return -1.0f; // IOS使用meta, 反过来
            // 主角坐下来时，也需要反过来，先不处理
            return 1.0f;
        }
        //load资源后设置
        private void OnLoadHudAtlas(){
            if(AtlasId!=0){//图片
                var texture = HudAtlasManager.GetInstance().GetTextureByAtlasId(AtlasId);
                m_kMaterial.SetTexture("_MainTex", texture);
                m_kMaterial.SetFloat("_ReverseY", GetReserveY());
            }
            else{//文字
                if(m_kFont!=null&&m_kFont.material!=null){
                    Material mat = m_kFont.material;
                    if (m_kMaterial == null)
                    {
                        m_kMaterial = new Material(GetFontShader());
                    }
                    m_kMaterial.mainTexture=mat.mainTexture;
                    m_kMaterial.mainTextureOffset=mat.mainTextureOffset;
                    m_kMaterial.mainTextureScale=mat.mainTextureScale;
                    m_kMaterial.SetFloat("_ReverseY",GetReserveY());

                }
            }
        }

        public void SetFont(UIFont font){
            if(m_kMaterial==null){
                m_kMaterial = new Material(GetFontShader());
            }
            m_kFont=font;
            Material mat=m_kFont.material;
            if(mat!=null){
                m_kMaterial.mainTexture=mat.mainTexture;
                m_kMaterial.mainTextureOffset=mat.mainTextureOffset;
                m_kMaterial.mainTextureScale=mat.mainTextureScale;
                m_kMaterial.SetFloat("_ReverseY",GetReserveY());
            }
        }

        public void Release(){
            ClearAllVertex();
            if(m_kMesh!=null){
                Object.Destroy(m_kMesh);
                m_kMesh=null;
            }

            if(m_kMaterial!=null){
                Object.Destroy(m_kMaterial);
                m_kMaterial=null;
            }
        }

        public void ClearAllVertex(){
            m_bDirty=true;
            m_bHaveNullVertex=false;
            m_kSpriteVertexs.Clear();
            ReleaseTexture();
        }
        // 功能：快速清队模型的顶点
        public void FastClearVertex(){
            m_bDirty=true;
            m_bHaveNullVertex=false;
            m_kSpriteVertexs.Clear();
        }
        
        public void PushHudVertex(HudVertex v){
            VertexDirty();
            v.m_iHudVertexIndex=m_kSpriteVertexs.size;//新增的顶点index 自己保存
            m_kSpriteVertexs.Add(v);
            if(!m_bQueryTexture){
                QueryTexture();//加载图片
            }
        }

        public void EraseHudVertex(HudVertex v){
            int index=v.m_iHudVertexIndex;
            if(index>=0&&index<m_kSpriteVertexs.size){
                //范围内
                if(m_kSpriteVertexs[index]!=null&&m_kSpriteVertexs[index].ID==v.ID){
                    VertexDirty();
                    m_bHaveNullVertex=true;
                    m_kSpriteVertexs[index]=null;
                    return;
                }
            }

            for(int i=0;i<m_kSpriteVertexs.size;i++){
                if(m_kSpriteVertexs[i]!=null&&m_kSpriteVertexs[i].ID==v.ID){
                    VertexDirty();
                    m_bHaveNullVertex=true;
                    m_kSpriteVertexs[i]=null;
                    break;
                }
            }
        }
        public void VertexDirty(){
            m_bDirty=true;
        }

        public bool IsDirty(){
            return m_bDirty;
        }

        private void ReleaseTexture(){
            if(m_bQueryTexture){
                m_bQueryTexture=false;
                HudAtlasManager.GetInstance().ReleaseTextureByAtlasId(AtlasId);
            }
        }

        public int SpriteNumb{
            get{
                return m_kSpriteVertexs.size;
            }
        }

        public int OldSpriteNumb{
            get{
                return m_iOldSpriteNumb;
            }
        }

        public void UpdateLogic(){
            if(!IsDirty()){
                return;
            }
            m_bDirty=false;
            if(m_bHaveNullVertex){
                m_bHaveNullVertex=false;
                m_kSpriteVertexs.ClearNullItem();
            }
            UpdateMesh();
            OnLoadHudAtlas();
            m_iOldSpriteNumb =m_kSpriteVertexs.size;
        }




        /// <summary>
        /// 旋转所有HUD顶点
        /// </summary>
        /// <param name="rotate">旋转角度（度）</param>
        public void RotateSpriteVertexs(float rotate)
        {
            if (m_kSpriteVertexs.size == 0) return;

            // 将角度转换为弧度
            float radians = rotate * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            // 遍历所有的HudVertex
            for (int i = 0; i < m_kSpriteVertexs.size; i++)
            {
                HudVertex vertex = m_kSpriteVertexs[i];
                if (vertex == null) continue;

                // 计算当前sprite的中心点（基于四个顶点的平均值）
                Vector2 center = (vertex.m_vVertexRU + vertex.m_vVertexRD + vertex.m_vVertexLD + vertex.m_vVertexLU) * 0.25f;

                // 旋转右上角顶点
                Vector2 ruRelative = vertex.m_vVertexRU - center;
                vertex.m_vVertexRU.x = center.x + ruRelative.x * cos - ruRelative.y * sin;
                vertex.m_vVertexRU.y = center.y + ruRelative.x * sin + ruRelative.y * cos;

                // 旋转右下角顶点
                Vector2 rdRelative = vertex.m_vVertexRD - center;
                vertex.m_vVertexRD.x = center.x + rdRelative.x * cos - rdRelative.y * sin;
                vertex.m_vVertexRD.y = center.y + rdRelative.x * sin + rdRelative.y * cos;

                // 旋转左下角顶点
                Vector2 ldRelative = vertex.m_vVertexLD - center;
                vertex.m_vVertexLD.x = center.x + ldRelative.x * cos - ldRelative.y * sin;
                vertex.m_vVertexLD.y = center.y + ldRelative.x * sin + ldRelative.y * cos;

                // 旋转左上角顶点
                Vector2 luRelative = vertex.m_vVertexLU - center;
                vertex.m_vVertexLU.x = center.x + luRelative.x * cos - luRelative.y * sin;
                vertex.m_vVertexLU.y = center.y + luRelative.x * sin + luRelative.y * cos;
            }

            // 标记为需要更新
            VertexDirty();
        }

        //写顶点数据
        private void FillVertex(){
            PrepareWrite(m_kSpriteVertexs.size * 4);
            Vector2 offset = Vector2.zero;
            float scaleX = 1.0f;
            float scaleY = 1.0f;
            float cameraScaleX = s_fCameraScaleX;
            float cameraScaleY = s_fCameraScaleY;

            Vector2 centerPos = Vector2.zero;

            //遍历每个顶点赋值
            for (int i = 0, size = m_kSpriteVertexs.size; i < size; i++)
            {
                HudVertex v = m_kSpriteVertexs[i];
                v.m_iHudVertexIndex = i;
                m_kVerts.Add(v.m_vWorldPos);
                m_kVerts.Add(v.m_vWorldPos);
                m_kVerts.Add(v.m_vWorldPos);
                m_kVerts.Add(v.m_vWorldPos);

                float radians = v.m_fRotate * Mathf.Deg2Rad;
                float cos = Mathf.Cos(radians);
                float sin = Mathf.Sin(radians);

                scaleX = cameraScaleX * v.m_fScale;
                scaleY = cameraScaleY * v.m_fScale;

                offset = v.m_vVertexRU;//右上角
                offset += v.m_vOffset;
                offset.x *= scaleX;
                offset.y *= scaleY;
                

                if (v.m_fRotate > 0f)
                {
                    //存在旋转
                    offset.x = centerPos.x + offset.x * cos - offset.y * sin;
                    offset.y = centerPos.y + offset.x * sin + offset.y * cos;
                }
                offset += v.m_vMove;

                m_kOffsets.Add(offset);

                offset = v.m_vVertexRD;//右下角
                offset += v.m_vOffset;
                offset.x *= scaleX;
                offset.y *= scaleY;
                


                if (v.m_fRotate > 0f)
                {
                    //存在旋转
                    offset.x = centerPos.x + offset.x * cos - offset.y * sin;
                    offset.y = centerPos.y + offset.x * sin + offset.y * cos;
                }
                offset += v.m_vMove;

                m_kOffsets.Add(offset);

                offset = v.m_vVertexLD;//左下角
                offset += v.m_vOffset;
                offset.x *= scaleX;
                offset.y *= scaleY;
                

                if (v.m_fRotate > 0f)
                {
                    //存在旋转
                    offset.x = centerPos.x + offset.x * cos - offset.y * sin;
                    offset.y = centerPos.y + offset.x * sin + offset.y * cos;
                }

                offset += v.m_vMove;
                m_kOffsets.Add(offset);


                offset = v.m_vVertexLU;//左上角
                offset += v.m_vOffset;
                offset.x *= scaleX;
                offset.y *= scaleY;
                

                if (v.m_fRotate > 0f)
                {
                    //存在旋转
                    offset.x = centerPos.x + offset.x * cos - offset.y * sin;
                    offset.y = centerPos.y + offset.x * sin + offset.y * cos;
                }
                offset += v.m_vMove;
                m_kOffsets.Add(offset);



                m_kUVs.Add(v.m_vUVRU);
                m_kUVs.Add(v.m_vUVRD);
                m_kUVs.Add(v.m_vUVLD);
                m_kUVs.Add(v.m_vUVLU);


                m_kColors.Add(v.m_vColorRU);
                m_kColors.Add(v.m_vColorRD);
                m_kColors.Add(v.m_vColorLD);
                m_kColors.Add(v.m_vColorLU);

            }
        }

        private void PrepareWrite(int vertexNum){
            m_kVerts.CleanPreWrite(vertexNum);
            m_kOffsets.CleanPreWrite(vertexNum);
            m_kUVs.CleanPreWrite(vertexNum);
            m_kColors.CleanPreWrite(vertexNum);
        }
        /// <summary>
        /// 填充顶点数据
        /// </summary>
        private void UpdateMesh(){
            int oldVertexCount=m_kVerts.size;
            FillVertex();

            //buffer里面数据空白的全部填充最后一个数据
            int last=m_kVerts.size-1;
            int exSize=m_kVerts.buffer.Length;
            int vertexCount=m_kVerts.size;
            if(last>=0){
                Vector3[]vertexs=m_kVerts.buffer;
                Vector2[]uv1s=m_kUVs.buffer;
                Vector2[]offsets=m_kOffsets.buffer;
                Color32[]colors=m_kColors.buffer;
                int max=m_kVerts.buffer.Length;
                for(int i=m_kVerts.size;i<max;i++){
                    vertexs[i]=vertexs[last];
                    uv1s[i]=uv1s[last];
                    offsets[i]=offsets[last];
                    colors[i]=colors[last];
                }
            }
            m_kVerts.size=exSize;
            m_kUVs.size=exSize;
            m_kOffsets.size=exSize;
            m_kColors.size=exSize;

            //更新索引数据
            bool rebuildIndices=oldVertexCount!=exSize;
            if(rebuildIndices){
                AdjustIndexs(vertexCount);
            }

            if(m_kMesh==null){
                m_kMesh=new Mesh(); 
                m_kMesh.name="hud_mesh";
                m_kMesh.hideFlags=HideFlags.DontSave;
                m_kMesh.MarkDynamic();
            }else if(rebuildIndices||m_kMesh.vertexCount!=m_kVerts.size){
                m_kMesh.Clear();
            }

            if(m_kMesh!=null){//也不可能=null
                m_kMesh.vertices=m_kVerts.buffer;
                m_kMesh.uv=m_kUVs.buffer;
                m_kMesh.uv2=m_kOffsets.buffer;
                m_kMesh.colors32=m_kColors.buffer;
                m_kMesh.triangles=m_kIndices.buffer;
                m_kMesh.RecalculateBounds();
                
            }
            //一个可用mesh生产好了
        }

        //三角形编号
        private void AdjustIndexs(int vertexCount){
            int oldSize=m_kIndices.size;
            int newSize=m_kVerts.size/4*6;
            m_kIndices.CleanPreWrite(vertexCount/4*6);
            //填充多余的
            int maxCount=m_kIndices.buffer.Length;
            int []indices=m_kIndices.buffer;

            int index=0;
            int i=0;
            for(; i <vertexCount ; i+=4) {
                //0 1 2  2 3 1两个三角形
                indices[index++]=i;
                indices[index++]=i+1;
                indices[index++]=i+2;

                indices[index++]=i+2;
                indices[index++]=i+3;
                indices[index++]=i;
            }//for

            int last=vertexCount-1;
            for(;index<maxCount;){
                indices[index++] = last;
                indices[index++] = last;
                indices[index++] = last;
                indices[index++] = last;
                indices[index++] = last;
                indices[index++] = last;

            }

            m_kIndices.size=index;
        }


        public void PushChar(Vector3 world,float screenX,float screenY,float localX,float localY,char ch, Color clrLeftUp, Color clrLeftDown, Color clrRightUp, Color clrRightDown){
            m_kFont.GetCharacterInfo(ch,ref m_tempCharInfo);
            float fl=localX;
            float fb=localY;
            float fr=localX+m_tempCharInfo.glyphWidth;
            float ft=localY+m_tempCharInfo.glyphHeight;

            fl=screenX+fl+m_fScale;
            fb=screenY+fb*m_fScale;
            fr=screenX+fr*m_fScale;
            ft=screenY+ft*m_fScale;

            Vector3[]vertexs={Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};
            vertexs[0].Set(fr,fb,0f);//右上角
            vertexs[1].Set(fr,ft,0f);//右下角
            vertexs[2].Set(fl,ft,0f);//左下角
            vertexs[3].Set(fl,fb,0f);//左上角

            //顶点
            m_kVerts.Add(world);
            m_kVerts.Add(world);
            m_kVerts.Add(world);
            m_kVerts.Add(world);
            //uv2
            m_kOffsets.Add(vertexs[0]);
            m_kOffsets.Add(vertexs[1]);
            m_kOffsets.Add(vertexs[2]);
            m_kOffsets.Add(vertexs[3]);
            //uv
            m_kUVs.Add(m_tempCharInfo.uvBottomRight);
            m_kUVs.Add(m_tempCharInfo.uvTopRight);
            m_kUVs.Add(m_tempCharInfo.uvTopLeft);
            m_kUVs.Add(m_tempCharInfo.uvBottomLeft);
            //顶点色
            m_kColors.Add(clrRightDown);
            m_kColors.Add(clrRightUp);
            m_kColors.Add(clrLeftUp);
            m_kColors.Add(clrLeftDown);
        }

    }

}
