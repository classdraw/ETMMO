using UnityEditor;
using UnityEngine;

/// <summary>
/// 同步 SR 系列材质 _ReceiveShadows 与 URP 关键字 _RECEIVE_SHADOWS_OFF。
/// </summary>
public class SR_ReceiveShadowsShaderGUI : ShaderGUI
{
    static readonly int ReceiveShadowsId = Shader.PropertyToID("_ReceiveShadows");

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUI.BeginChangeCheck();
        materialEditor.PropertiesDefaultGUI(properties);
        if (EditorGUI.EndChangeCheck())
        {
            SyncKeywords(materialEditor);
        }
        else
        {
            SyncKeywords(materialEditor);
        }
    }

    static void SyncKeywords(MaterialEditor materialEditor)
    {
        foreach (Object target in materialEditor.targets)
        {
            if (target is not Material material)
            {
                continue;
            }

            bool receive = material.HasProperty(ReceiveShadowsId) && material.GetFloat(ReceiveShadowsId) > 0.5f;
            SetKeyword(material, "_RECEIVE_SHADOWS_OFF", !receive);
        }
    }

    static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
        {
            material.EnableKeyword(keyword);
        }
        else
        {
            material.DisableKeyword(keyword);
        }
    }
}
