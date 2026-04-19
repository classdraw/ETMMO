namespace ET.Client
{
    /// <summary>
    /// 登录相关 RPC 结果：先看你 bool，失败时再看 ErrorCode；无服务端错误码时用 <see cref="ET.ErrorCode.ERR_None"/>。
    /// </summary>
    public struct LoginOperationResult
    {
        public bool Ok;
        public int ErrorCode;

        public static LoginOperationResult Success()
        {
            return new LoginOperationResult { Ok = true, ErrorCode = ET.ErrorCode.ERR_None };
        }

        public static LoginOperationResult Fail(int errorCode = ET.ErrorCode.ERR_None)
        {
            return new LoginOperationResult { Ok = false, ErrorCode = errorCode };
        }
    }
}
