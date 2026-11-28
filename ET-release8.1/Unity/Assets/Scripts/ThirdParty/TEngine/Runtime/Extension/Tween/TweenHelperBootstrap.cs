namespace TEngine
{
    /// <summary>在 Hotfix 使用 <see cref="Utility.Tween"/> 前注册默认动画助手。</summary>
    public static class TweenHelperBootstrap
    {
        private static bool initialized;

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            TweenDriver.EnsureExists();
            Utility.Tween.SetTweenHelper(new DefaultTweenHelper());
        }
    }
}