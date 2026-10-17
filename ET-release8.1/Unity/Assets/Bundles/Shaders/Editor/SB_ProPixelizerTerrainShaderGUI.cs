using System;
using UnityEditor;
using UnityEngine;

public class SB_ProPixelizerTerrainShaderGUI : ShaderGUI
{
    const string TerrainGuiTypeName = "UnityEditor.Rendering.Universal.TerrainLitShaderGUI, Unity.RenderPipelines.Universal.Editor";

    static ShaderGUI _terrainGui;

    static ShaderGUI TerrainGui
    {
        get
        {
            if (_terrainGui != null)
                return _terrainGui;

            Type terrainGuiType = Type.GetType(TerrainGuiTypeName);
            if (terrainGuiType != null && typeof(ShaderGUI).IsAssignableFrom(terrainGuiType))
                _terrainGui = (ShaderGUI)Activator.CreateInstance(terrainGuiType);

            return _terrainGui;
        }
    }

    static readonly int ColorGradingId = Shader.PropertyToID("_COLOR_GRADING");
    static readonly int DitheringId = Shader.PropertyToID("_PROPIXELIZER_DITHERING");
    static readonly int ReceiveShadowsId = Shader.PropertyToID("_RECEIVE_SHADOWS");
    static readonly int UseObjectPosId = Shader.PropertyToID("_USE_OBJECT_POSITION_FOR_PIXEL_GRID");

    static readonly string[] ProPixelizerPropertyPrefixes =
    {
        "_Outline", "_Edge", "_LightingRamp", "_PaletteLUT", "_AmbientLight", "_LightingRampRange",
        "_ShadowTintColor", "_PixelSize", "_PixelGridOrigin", "_ID", "_COLOR_GRADING", "_PROPIXELIZER_DITHERING",
        "_RECEIVE_SHADOWS", "_USE_OBJECT_POSITION_FOR_PIXEL_GRID", "_AlphaClipThreshold",
        "_UseSceneShadow", "_SceneShadow"
    };

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        EditorGUILayout.LabelField("ProPixelizer (Terrain)", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        bool changed = false;
        foreach (MaterialProperty prop in properties)
        {
            if (!IsProPixelizerProperty(prop.name))
                continue;
            materialEditor.ShaderProperty(prop, prop.displayName);
            changed = true;
        }
        EditorGUI.indentLevel--;

        if (changed)
            EditorGUILayout.Space();

        if (TerrainGui != null)
            TerrainGui.OnGUI(materialEditor, properties);
        else
            EditorGUILayout.HelpBox("未找到 URP TerrainLitShaderGUI。请在 Terrain 组件上绘制图层，或使用 Paint Terrain 工具。", MessageType.Info);

        if (material != null)
            SyncKeywords(material);
    }

    static bool IsProPixelizerProperty(string name)
    {
        for (int i = 0; i < ProPixelizerPropertyPrefixes.Length; i++)
        {
            if (name.StartsWith(ProPixelizerPropertyPrefixes[i]))
                return true;
        }
        return false;
    }

    static void SyncKeywords(Material material)
    {
        SetKeyword(material, "_COLOR_GRADING_ON", material.GetFloat(ColorGradingId) > 0.5f);
        SetKeyword(material, "_PROPIXELIZER_DITHERING_ON", material.GetFloat(DitheringId) > 0.5f);
        SetKeyword(material, "_RECEIVE_SHADOWS_ON", material.GetFloat(ReceiveShadowsId) > 0.5f);
        SetKeyword(material, "USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON", material.GetFloat(UseObjectPosId) > 0.5f);
    }

    static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
            material.EnableKeyword(keyword);
        else
            material.DisableKeyword(keyword);
    }
}
