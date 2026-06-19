using UnityEditor;
using HybridCLR.Editor.Commands;
using System.IO;
using XEngine.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using HybridCLR.Editor;
using System.Collections.Generic;
using UnityEngine;
using XEngine.Hud;
using EditorTools.UI;

public class BuildAssetsEditor 
{
    private static List<string> s_Files = new List<string>();
    private static List<string> m_HotList=new List<string>(){
        "Update_Common","Update_Display","Update_Game"
    };

    #region 资源构建相关
    [MenuItem("Tools/Asset/CollectAllTextAsset", priority = 2)]
    public static void CollectAllTextAsset(){
        s_Files.Clear();
        string dir = Application.dataPath+ "/Editor/MyGameAssets/GameRes/LocalData/";
        CalcFilesInDir(dir);

        var textAssetConfig = AssetDatabase.LoadAssetAtPath<TextAssetConfig>("Assets/Editor/MyGameAssets/GameRes/ScriptObject/TextAssetConfig.asset");
        textAssetConfig.m_kTextAssets.Clear();

        foreach (var f in s_Files) {
            var txtAssets = AssetDatabase.LoadAssetAtPath<TextAsset>(f.Replace(Application.dataPath, "Assets/"));
            var s = f.Replace(dir, "").Replace(".bytes","");
            textAssetConfig.m_kTextAssets.Add(new TextAssetData(){
                m_kTextAsset=txtAssets,
                m_Name=s
            });
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    private static void CalcFilesInDir(string dir) {
        string[] files = Directory.GetFiles(dir);
        if (files!=null) {
            foreach (var f in files) {
                if (f.EndsWith(".meta")) {
                    continue;
                }
                s_Files.Add(f);
            }
            
        }

        string[] dirs = Directory.GetDirectories(dir);
        if (dirs!=null) {
            for (int i=0; i<dirs.Length;i++) {
                CalcFilesInDir(dirs[i]);
            }
        }
    }

    [MenuItem("Tools/Asset/CreateSpriteAtlas", priority = 0)]
    private static void CreateSpriteAtlas()
    {
        var filePath = EditorUtility.OpenFolderPanel("选择打开文件", UnityEngine.Application.dataPath+ "/AssetArt/UI/_Hud", "*");
        if (string.IsNullOrEmpty(filePath)) {
            return;
        }  
        string repPath = Application.dataPath;
        var strs=IconProcessor.GetAssetPaths(filePath);
        for (int i=0; i<strs.Length;i++) {
            string ss = strs[i];
            ss="Assets/"+ss.Replace(repPath,"");
            strs[i] = ss;
        }
        
        string outPath = "Assets/Art/HudOut/out.png";

        if (File.Exists(outPath)) { 
            File.Delete(outPath);
        }

        Debug.Log(outPath);
        IconProcessor.CreateFolderModeAtlas(strs, outPath);
        AssetDatabase.Refresh();
        //string resPath = Application.dataPath + "/out.png";
        //if (File.Exists(resPath)) { 
        //     File.Copy(resPath, outPath);
        // }


    }
    //把图集解析 生成图片信息
    [MenuItem("Tools/Asset/CalcSpriteAtlasInfo", priority = 1)]
    private static void CalcSpriteAtlasInfo() {

        var texturePath= EditorUtility.OpenFilePanel("选择打开文件", UnityEngine.Application.dataPath + "/AssetArt/UI/_HudOut", "*");
        if (string.IsNullOrEmpty(texturePath))
        {
            return;
        }
        string repPath = Application.dataPath;
        texturePath ="Assets/"+ texturePath.Replace(repPath, "");
        TextureImporter textureImporter = TextureImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter==null) { 
            return;
        }
        string scPath = texturePath.Replace(".png",".asset");
        if (File.Exists(scPath))
        {
            File.Delete(scPath);
        }

        
        if (textureImporter.textureType == TextureImporterType.Sprite)
        {
            SpriteAtlasConfig spriteAtlasConfig = new SpriteAtlasConfig();
            textureImporter.GetSourceTextureWidthAndHeight(out int width,out int height);
            spriteAtlasConfig.m_iWidth = width;
            spriteAtlasConfig.m_iHeight = height;
            spriteAtlasConfig.m_kSprites = new List<SpriteInfo>();
            spriteAtlasConfig.m_kTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            int index = 0;
            foreach (SpriteMetaData spriteData in textureImporter.spritesheet)
            {
                //Debug.Log($"Sprite Name: {spriteData.name}");
                //Debug.Log($"Rect: {spriteData.rect}");
                //Debug.Log($"Border: {spriteData.border}");
                // 更多信息...

                SpriteInfo spriteInfo = new SpriteInfo();
                spriteInfo.m_sSpriteName=spriteData.name;
                spriteInfo.m_kRect=spriteData.rect;
                spriteInfo.m_iIndex=index++;
                float w = spriteInfo.m_kRect.width;
                float h=spriteInfo.m_kRect.height;
                float x=spriteInfo.m_kRect.x;
                float y=spriteInfo.m_kRect.y;

                Vector4 uv = new Vector4(x/width,y/height,(x+w)/width,(y+h)/height);
                spriteInfo.m_kUV = uv;
                spriteInfo.m_kBorder = spriteData.border;
                spriteAtlasConfig.m_kSprites.Add(spriteInfo);
            }

            AssetDatabase.CreateAsset(spriteAtlasConfig,scPath);
        }
    }
    #endregion
}
