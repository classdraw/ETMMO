using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 测试用的加载器，后续可以换框架自定义的
/// </summary>
public class TestLoader
{
    public static UnityEngine.Object Load(string name) {
        var obj= Resources.Load(name);
        return obj;
    }

    public static T Load<T>(string name) where T:UnityEngine.Object{
        var obj = Resources.Load(name);
        return obj as T;
    }
}
