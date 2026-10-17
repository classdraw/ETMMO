using UnityEditor;
using UnityEngine;

public class SB_ProPixelizerUberShaderGUI : ShaderGUI
{
    static readonly int ColorGradingId = Shader.PropertyToID("_COLOR_GRADING");
    static readonly int DitheringId = Shader.PropertyToID("_PROPIXELIZER_DITHERING");
    static readonly int ReceiveShadowsId = Shader.PropertyToID("_RECEIVE_SHADOWS");
    static readonly int UseObjectPosId = Shader.PropertyToID("_USE_OBJECT_POSITION_FOR_PIXEL_GRID");
    static readonly int WorkflowId = Shader.PropertyToID("_WorkflowMode");
    static readonly int SpecularSetupKeyword = Shader.PropertyToID("_SPECULAR_SETUP");

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        EditorGUI.BeginChangeCheck();
        foreach (MaterialProperty prop in properties)
        {
            if (prop.name is "_MainTex")
                continue;
            materialEditor.ShaderProperty(prop, prop.displayName);
        }

        if (EditorGUI.EndChangeCheck() && material != null)
        {
            SyncKeywords(material);
        }
        else if (material != null)
        {
            SyncKeywords(material);
        }
    }

    static void SyncKeywords(Material material)
    {
        SetKeyword(material, "_COLOR_GRADING_ON", material.GetFloat(ColorGradingId) > 0.5f);
        SetKeyword(material, "_PROPIXELIZER_DITHERING_ON", material.GetFloat(DitheringId) > 0.5f);
        SetKeyword(material, "_RECEIVE_SHADOWS_ON", material.GetFloat(ReceiveShadowsId) > 0.5f);
        SetKeyword(material, "USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON", material.GetFloat(UseObjectPosId) > 0.5f);
        SetKeyword(material, "_SPECULAR_SETUP", material.GetFloat(WorkflowId) > 0.5f);
    }

    static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
            material.EnableKeyword(keyword);
        else
            material.DisableKeyword(keyword);
    }
}
