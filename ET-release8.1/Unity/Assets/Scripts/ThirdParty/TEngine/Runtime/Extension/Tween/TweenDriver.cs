using UnityEngine;

namespace TEngine
{
    /// <summary>驱动 <see cref="DefaultTweenHelper"/> 的逐帧更新。</summary>
    [DefaultExecutionOrder(-500)]
    internal sealed class TweenDriver : MonoBehaviour
    {
        private static TweenDriver instance;

        public static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject go = new GameObject(nameof(TweenDriver));
            DontDestroyOnLoad(go);
            instance = go.AddComponent<TweenDriver>();
        }

        private void Update()
        {
            DefaultTweenHelper.Tick(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }
}