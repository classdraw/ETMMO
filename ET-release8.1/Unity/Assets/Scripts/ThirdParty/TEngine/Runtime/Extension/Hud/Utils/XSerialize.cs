using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace XEngine.Hud{

    public interface ISerializable{
        void Serialize(CSerialize ar);
    }

    public enum SerializeType{
        read,
        write,
        append,
    }

    class CSerialzieStream:Stream{
        byte[] m_kData;
        int m_iSize;
        int m_iSizeMax;
        int m_iPos;
        SerializeType m_eSerializeType;
        private bool m_bLittleByte;
        public CSerialzieStream(SerializeType arType)
        {
            ResetStream(arType);
        }
        public CSerialzieStream(SerializeType arType, byte[] buffer, int nBufSize)
        {
            ResetStream(arType, buffer, nBufSize);
        }
        public void ResetStream(SerializeType arType)
        {
            m_iSize = 0;
            m_iPos = 0;
            m_eSerializeType = arType;
            m_bLittleByte = System.BitConverter.IsLittleEndian;
        }

        public void ResetStream(SerializeType arType, byte[] buffer, int bufSize)
        {
            if(arType==SerializeType.read){//只读不需要拷贝
                m_kData=buffer;
                m_iSize=m_iSizeMax=bufSize;
            }else{
                m_kData=new byte[bufSize];
                if(buffer!=null){
                    Array.Copy(buffer,m_kData,bufSize);
                }
                m_iSize=m_iSizeMax=bufSize;
            }

            m_iPos=0;
            m_eSerializeType=arType;
            m_bLittleByte= System.BitConverter.IsLittleEndian;
        }
        //存储扩容
        private void reserve(int sizeMax){
            if(m_iSizeMax<sizeMax){
                var tempData=new byte[sizeMax];
                m_iSizeMax=sizeMax;
                if(m_iSize>0){
                    Array.Copy(m_kData,tempData,m_iSize);
                }
                m_kData=tempData;
            }
        }

        //自增长
        private void auto_grow(int growSize){
            if (growSize > 0 && m_iPos + growSize > m_iSizeMax)//增长的超过上限
            {
                int newSize=m_iSizeMax*2+ (growSize + 4095) / 4096 * 4096;
                if (newSize < 4096)//上限
                    newSize = 4096;

                    reserve(newSize);
            }
        }
        // 功能：交换当前INT变量
        // 参数：intSize - 整数的字节数
        private void swap_int(int intSize,int pos){
            byte temp=0;
            switch(intSize){
                case 2:// [1][2] ==> [2][1]
                    temp = m_kData[pos]; 
                    m_kData[pos] = m_kData[pos + 1];
                    m_kData[pos + 1]=temp;
                break;
                case 4:// [1][2][3][4] ==> [4][3][2][1]
                    temp = m_kData[pos]; m_kData[pos] = m_kData[pos + 3]; m_kData[pos + 3] = temp;//1和4交换
                    temp = m_kData[pos + 1]; m_kData[pos + 1] = m_kData[pos + 2]; m_kData[pos + 2] = temp;//2和3交换
                break;
                case 8:  // [1][2][3][4][5][6][7][8] ==> [8][7][6][5][4][3][2][1]
                    temp = m_kData[pos]; m_kData[pos] = m_kData[pos + 7]; m_kData[pos + 7] = temp;//1和8
                    temp = m_kData[pos + 1]; m_kData[pos + 1] = m_kData[pos + 6]; m_kData[pos + 6] = temp;//2和7
                    temp = m_kData[pos + 2]; m_kData[pos + 2] = m_kData[pos + 5]; m_kData[pos + 5] = temp;//3和6
                    temp = m_kData[pos + 3]; m_kData[pos + 3] = m_kData[pos + 4]; m_kData[pos + 4] = temp;//4和5
                break;
                default:break;
            }
        }

        void AutoSwapInt(int intSize,int pos){
            if(!m_bLittleByte){
                swap_int(intSize,pos);
            }
        }

        public byte[] GetBuffer(){
            return m_kData;
        }

        public int GetBufferSize(){
            return m_iPos;
        }

        public override bool CanRead{get{return m_eSerializeType==SerializeType.read;}}
        public override bool CanSeek{get{return true;}}
        public override bool CanWrite { get { return m_eSerializeType == SerializeType.read || m_eSerializeType == SerializeType.append; } }
        public override long Length { get { return m_iSize; } }
        public override long Position
        {
            get { return m_iPos; }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush()
        {
            // 写入文件
            m_iPos = 0;
            m_iSize = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(m_iPos+count>m_iSize){
                count=m_iSize-m_iPos;
            }

            if(count>0){
                CSerialzieStream.CopyArray(m_kData,m_iPos,ref buffer,offset,count);
                m_iPos+=count;
            }
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    m_iPos = (int)offset;
                    break;
                case SeekOrigin.Current:
                    m_iPos += (int)offset;
                    break;
                case SeekOrigin.End:
                    m_iPos = m_iSize - (int)offset;
                    break;
                default:
                    break;
            }
            if (m_iPos < 0)
                m_iPos = 0;
            else if (m_iPos > m_iSizeMax)
                m_iPos = m_iSizeMax;
            if (m_eSerializeType == SerializeType.write
                || m_eSerializeType == SerializeType.append)
            {
                if (m_iSize < m_iPos)
                    m_iSize = m_iPos;
            }
            return m_iPos;
        }

        public override void SetLength(long value)
        {
            m_iSize = (int)value;
            if (m_iSize < 0)
                m_iSize = 0;
            if (m_iSize > m_iSizeMax)
                m_iSize = m_iSizeMax;
            if (m_iPos > m_iSize)
                m_iPos = m_iSize;
        }

        public static void CopyArray(byte[] src, int nSrcOffset, ref byte[] des, int nDesOffset, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                des[i + nDesOffset] = src[i + nSrcOffset];
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            auto_grow(count);
            if (count > 0)
            {
                CSerialzieStream.CopyArray(buffer, offset, ref m_kData, m_iPos, count);
                m_iPos += count;
                if (m_iSize < m_iPos)
                    m_iSize = m_iPos;
            }
        }

        public void ReadInt(ref int nValue)
        {
            if (m_iPos + 4 <= m_iSizeMax)
            {
                nValue = System.BitConverter.ToInt32(m_kData, m_iPos);
                m_iPos += 4;
            }
        }

        
        public void ReadInt(ref uint nValue)
        {
            if (m_iPos + 4 <= m_iSizeMax)
            {
                nValue = System.BitConverter.ToUInt32(m_kData, m_iPos);
                m_iPos += 4;
            }
        }

        public void ReadInt(ref long nValue)
        {
            if (m_iPos + 8 <= m_iSizeMax)
            {
                nValue = System.BitConverter.ToInt64(m_kData, m_iPos);
                m_iPos += 8;
            }
        }
        public void ReadInt(ref ulong nValue)
        {
            if (m_iPos + 8 <= m_iSizeMax)
            {
                nValue = System.BitConverter.ToUInt64(m_kData, m_iPos);
                m_iPos += 8;
            }
        }
        public void ReadInt(ref byte yValue)
        {
            if (m_iPos + 1 <= m_iSizeMax)
            {
                yValue = m_kData[m_iPos++];
            }
        }
        public void ReadInt(ref short wValue)
        {
            if (m_iPos + 2 <= m_iSizeMax)
            {
                wValue = System.BitConverter.ToInt16(m_kData, m_iPos);
                m_iPos += 2;
            }
        }
        public void ReadInt(ref ushort wValue)
        {
            if (m_iPos + 2 <= m_iSizeMax)
            {
                wValue = System.BitConverter.ToUInt16(m_kData, m_iPos);
                m_iPos += 2;
            }
        }
        public void ReadInt(ref bool bValue)
        {
            if (m_iPos + 1 <= m_iSizeMax)
            {
                bValue = System.BitConverter.ToBoolean(m_kData, m_iPos);
                m_iPos++;
            }
        }
        public void ReadInt(ref float fValue)
        {
            if (m_iPos + 4 <= m_iSizeMax)
            {
                fValue = System.BitConverter.ToSingle(m_kData, m_iPos);
                m_iPos += 4;
            }
        }
        public void ReadInt(ref double fValue)
        {
            if (m_iPos + 8 <= m_iSizeMax)
            {
                fValue = System.BitConverter.ToDouble(m_kData, m_iPos);
                m_iPos += 8;
            }
        }
        public void ReadInt(ref Color32 color)
        {
            // a, r, g, b
            if(m_iPos + 4 <= m_iSizeMax)
            {
                color.a = m_kData[m_iPos++];
                color.r = m_kData[m_iPos++];
                color.g = m_kData[m_iPos++];
                color.b = m_kData[m_iPos++];
            }
        }

        public void ReadString(ref string str)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen >= 0 && m_iPos + nLen <= m_iSizeMax)
            {
                if (nLen > 0)
                {
                    str = System.Text.Encoding.UTF8.GetString(m_kData, m_iPos, nLen);
                    m_iPos += nLen;
                }
                else
                {
                    str = "";
                }
            }
            else
            {
                str = string.Empty;
            }
        }
        public void ReadStringUTF8(ref string str, int nLen)
        {
            if (nLen >= 0 && m_iPos + nLen <= m_iSizeMax)
            {
                if (nLen > 0)
                {
                    int nI = m_iPos;
                    int nEnd = m_iPos + nLen;
                    int nRealLen = 0;
                    for (; nI < nEnd; ++nI)
                    {
                        if (m_kData[nI] == 0)
                            break;
                        ++nRealLen;
                    }
                    str = System.Text.Encoding.UTF8.GetString(m_kData, m_iPos, nLen);
                }
                else
                {
                    str = "";
                }
            }
            else
                str = string.Empty;
            m_iPos += nLen;
        }
        public void ReadStringUTF32(ref string str, int nLen)
        {
            nLen *= 4;
            if (nLen >= 0 && m_iPos + nLen <= m_iSizeMax)
            {
                if (nLen > 0)
                {
                    // 只取有效的字符
                    int nRealLen = 0;
                    int nI = m_iPos;
                    for (; nRealLen < nLen;)
                    {
                        if (m_kData[nI] == 0
                            && m_kData[nI + 1] == 0
                            && m_kData[nI + 2] == 0
                            && m_kData[nI + 3] == 0)
                        {
                            break;
                        }
                        nI += 4;
                        nRealLen += 4;
                    }
                    str = System.Text.Encoding.UTF32.GetString(m_kData, m_iPos, nRealLen);
                }
                else
                {
                    str = "";
                }
            }
            else
                str = string.Empty;
            m_iPos += nLen;
        }
        // 功能：读取整数数组
        public void ReadIntArray(ref byte[] vArray)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen > 0 && m_iPos + nLen <= m_iSizeMax)
            {
                vArray = new byte[nLen];
                for (int i = 0; i < nLen; ++i)
                {
                    vArray[i] = m_kData[m_iPos++];
                }
            }
            else
            {
                vArray = null;
            }
        }
        // 功能：读取整数数组
        public void ReadIntArray(ref int[] vArray)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen > 0 && m_iPos + nLen * 4 <= m_iSizeMax)
            {
                vArray = new int[nLen];
                for (int i = 0; i < nLen; ++i)
                {
                    vArray[i] = 0;
                    ReadInt(ref vArray[i]);
                }
            }
            else
            {
                vArray = null;
            }
        }
        public void ReadIntArray(ref uint[] vArray)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen > 0 && m_iPos + nLen * 4 <= m_iSizeMax)
            {
                vArray = new uint[nLen];
                for (int i = 0; i < nLen; ++i)
                {
                    vArray[i] = 0;
                    ReadInt(ref vArray[i]);
                }
            }
            else
            {
                vArray = null;
            }
        }
        // 功能：读取浮点数组
        public void ReadIntArray(ref float[] vArray)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen > 0 && m_iPos + nLen * 4 <= m_iSizeMax)
            {
                vArray = new float[nLen];
                for (int i = 0; i < nLen; ++i)
                {
                    ReadInt(ref vArray[i]);
                }
            }
            else
            {
                vArray = null;
            }
        }
        // 功能：读取字符数组
        public void ReadIntArray(ref string[] vArray)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen > 0 && nLen * 4 <= m_iSizeMax)
            {
                vArray = new string[nLen];
                for (int i = 0; i < nLen; ++i)
                {
                    ReadString(ref vArray[i]);
                }
            }
            else
            {
                vArray = null;
            }
        }

        public void ReadIntArray(ref Vector3[] vArray)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen > 0 && nLen * 12 <= m_iSizeMax)
            {
                vArray = new Vector3[nLen];
                for (int i = 0; i < nLen; ++i)
                {
                    ReadInt(ref vArray[i].x);
                    ReadInt(ref vArray[i].y);
                    ReadInt(ref vArray[i].z);
                }
            }
            else
            {
                vArray = null;
            }
        }
        public void ReadIntArray(ref Color32[] vArray)
        {
            int nLen = 0;
            ReadInt(ref nLen);
            if (nLen > 0 && nLen * 4 + m_iPos <= m_iSizeMax)
            {
                vArray = new Color32[nLen];
                // a, r, g, b
                int nReadLen = nLen * 4;
                if (nReadLen > m_iSizeMax - m_iPos)
                    nReadLen = m_iSizeMax - m_iPos;
                nReadLen /= 4;
                for (int i = 0; i < nReadLen; ++i)
                {
                    vArray[i].a = m_kData[m_iPos++];
                    vArray[i].r = m_kData[m_iPos++];
                    vArray[i].g = m_kData[m_iPos++];
                    vArray[i].b = m_kData[m_iPos++];
                }
            }
            else
            {
                vArray = null;
            }
        }

        public void ReadArray(ref byte []vArray, int nLen)
        {
            if (vArray == null)
                vArray = new byte[nLen];
            for (int i = 0; i< nLen && m_iPos < m_iSizeMax; ++i)
            {
                vArray[i] = m_kData[m_iPos++];
            }
        }

        public void WriteInt(int nValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(nValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(uint nValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(nValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(long nValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(nValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(ulong nValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(nValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(byte yValue)
        {
            auto_grow(1);
            m_kData[m_iPos++] = yValue;
            if (m_iSize < m_iPos)
                m_iSize = m_iPos;
        }
        public void WriteInt(short wValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(wValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(ushort wValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(wValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(bool bValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(bValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(float fValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(fValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(double fValue)
        {
            byte[] byBuf = System.BitConverter.GetBytes(fValue);
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteInt(Color32 color)
        {
            byte[] byBuf = { color.a, color.r, color.g, color.b };
            Write(byBuf, 0, byBuf.Length);
        }
        public void WriteString(string str)
        {
            int nLen = 0;
            if (!string.IsNullOrEmpty(str))
            {
                byte[] byBuf = System.Text.Encoding.UTF8.GetBytes(str);
                nLen = byBuf.Length;
                WriteInt(nLen);
                auto_grow(nLen);
                Array.Copy(byBuf, 0, m_kData, m_iPos, nLen);
                m_iPos += nLen;
                if (m_iSize < m_iPos)
                    m_iSize = m_iPos;
            }
            else
            {
                WriteInt(nLen);
            }
        }
        public void PushTextString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                byte[] byBuf = System.Text.Encoding.UTF8.GetBytes(str);
                int nLen = byBuf.Length;
                auto_grow(nLen);
                Array.Copy(byBuf, 0, m_kData, m_iPos, nLen);
                m_iPos += nLen;
                if (m_iSize < m_iPos)
                    m_iSize = m_iPos;
            }
        }
        public void PushTextString(byte[] str)
        {
            int nLen = str != null ? str.Length : 0;
            if (nLen > 0)
            {
                auto_grow(nLen);
                Array.Copy(str, 0, m_kData, m_iPos, nLen);
                //for (int i = 0; i < nLen; ++i)
                //{
                //    m_kData[i + m_iPos] = (byte)(str[i]);
                //}
                m_iPos += nLen;
                if (m_iSize < m_iPos)
                    m_iSize = m_iPos;
            }
        }
        public void WriteStringUTF8(string str, int nSize)
        {
            auto_grow(nSize);
            int nCurWriteSize = 0;
            if (!string.IsNullOrEmpty(str))
            {
                byte[] byBuf = System.Text.Encoding.UTF8.GetBytes(str);
                nCurWriteSize = byBuf.Length;
                if (nCurWriteSize > nSize)
                    nCurWriteSize = nSize;
                Array.Copy(byBuf, 0, m_kData, m_iPos, nCurWriteSize);
            }
            m_iPos += nSize;
            if (m_iSize < m_iPos)
                m_iSize = m_iPos;
        }
        public void WriteStringUTF32(string str, int nSize)
        {
            nSize *= 4;
            auto_grow(nSize);
            int nCurWriteSize = 0;
            if (!string.IsNullOrEmpty(str))
            {
                byte[] byBuf = System.Text.Encoding.UTF32.GetBytes(str);
                nCurWriteSize = byBuf.Length;
                if (nCurWriteSize > nSize)
                    nCurWriteSize = nSize;
                Array.Copy(byBuf, 0, m_kData, m_iPos, nCurWriteSize);
            }
            m_iPos += nSize;
            if (m_iSize < m_iPos)
                m_iSize = m_iPos;
        }
        public void WriteIntArray(byte[] vArray)
        {
            int nLen = vArray != null ? vArray.Length : 0;
            WriteInt(nLen);
            if (nLen > 0)
            {
                auto_grow(nLen);
                for (int i = 0; i < nLen; ++i)
                {
                    m_kData[i + m_iPos] = (byte)(vArray[i]);
                }
                m_iPos += nLen;
                if (m_iSize < m_iPos)
                    m_iSize = m_iPos;
            }
        }
        // 功能：写入整数数组
        public void WriteIntArray(int[] vArray)
        {
            int nLen = vArray != null ? vArray.Length : 0;
            WriteInt(nLen);
            if (nLen > 0)
            {
                for (int i = 0; i < nLen; ++i)
                {
                    WriteInt(vArray[i]);
                }
            }
        }
        // 功能：写入整数数组
        public void WriteIntArray(uint[] vArray)
        {
            int nLen = vArray != null ? vArray.Length : 0;
            WriteInt(nLen);
            if (nLen > 0)
            {
                for (int i = 0; i < nLen; ++i)
                {
                    WriteInt(vArray[i]);
                }
            }
        }
        // 功能：写入浮点数组
        public void WriteIntArray(float[] vArray)
        {
            int nLen = vArray != null ? vArray.Length : 0;
            WriteInt(nLen);
            if (nLen > 0)
            {
                for (int i = 0; i < nLen; ++i)
                {
                    WriteInt(vArray[i]);
                }
            }
        }
        public void WriteIntArray(Vector3[] vArray)
        {
            int nLen = vArray != null ? vArray.Length : 0;
            WriteInt(nLen);
            if (nLen > 0)
            {
                for (int i = 0; i < nLen; ++i)
                {
                    WriteInt(vArray[i].x);
                    WriteInt(vArray[i].y);
                    WriteInt(vArray[i].z);
                }
            }
        }
        // 功能：写入字符串数组
        public void WriteIntArray(string[] vArray)
        {
            int nLen = vArray != null ? vArray.Length : 0;
            WriteInt(nLen);
            if (nLen > 0)
            {
                for (int i = 0; i < nLen; ++i)
                {
                    WriteString(vArray[i]);
                }
            }
        }
        // 功能：写入颜色数组
        public void WriteIntArray(Color32 []vArray)
        {
            int nLen = vArray != null ? vArray.Length : 0;
            WriteInt(nLen);
            if (nLen > 0)
            {
                auto_grow(nLen * 4);
                for (int i = 0; i < nLen; ++i)
                {
                    // a, r, g, b
                    m_kData[m_iPos++] = vArray[i].a;
                    m_kData[m_iPos++] = vArray[i].r;
                    m_kData[m_iPos++] = vArray[i].g;
                    m_kData[m_iPos++] = vArray[i].b;
                }
                if (m_iSize < m_iPos)
                    m_iSize = m_iPos;
            }
        }

        public void WriteArray(byte[] vArray, int nLen)
        {
            auto_grow(nLen);
            int nRealSize = vArray != null ? vArray.Length : 0;
            if (nRealSize > nLen)
                nRealSize = nLen;
            for (int i = 0; i < nRealSize && m_iPos < m_iSizeMax; ++i)
            {
                m_kData[m_iPos++] = vArray[i];
            }
            m_iPos += nLen - nRealSize;
            if (m_iSize < m_iPos)
                m_iSize = m_iPos;
        }
    }

    //一个内存序列化类
    public class CSerialize
    {
        CSerialzieStream m_kFile;
        SerializeType m_eSerializeType;
        string m_sFileName;
        int m_iVersion;

        bool m_bCreate=false;

        public CSerialize(SerializeType serializeType){
            m_eSerializeType=serializeType;
            m_kFile=new CSerialzieStream(serializeType);
            m_iVersion=0;
        }

        public CSerialize(SerializeType serializeType, byte[] buffer, int nBufSize)
        {
            m_eSerializeType = serializeType;
            m_kFile = new CSerialzieStream(serializeType, buffer, nBufSize);
            m_iVersion = 0;
        }

        public CSerialize(SerializeType arType, string szFileName)
        {
            m_eSerializeType = arType;
            m_sFileName = szFileName;
            m_iVersion = 0;
            if (m_eSerializeType == SerializeType.read)
            {
                if (System.IO.File.Exists(m_sFileName))
                {
                    byte[] fileData = System.IO.File.ReadAllBytes(m_sFileName);
                    m_kFile = new CSerialzieStream(arType, fileData, fileData.Length);
                }
                else
                    m_kFile = new CSerialzieStream(arType, null, 0);
            }
            else
            {
                m_kFile = new CSerialzieStream(arType);
            }
        }

            // 功能：重置流
        public void ResetStream(SerializeType arType)
        {
            m_eSerializeType = arType;
            if(m_kFile == null)
                m_kFile = new CSerialzieStream(arType);
            m_kFile.ResetStream(arType);
            m_iVersion = 0;
        }

        public void ResetStream(SerializeType arType, byte[] buffer, int nBufSize)
        {
            m_eSerializeType = arType;
            if (m_kFile == null)
                m_kFile = new CSerialzieStream(arType, buffer, nBufSize);
            else
                m_kFile.ResetStream(arType, buffer, nBufSize);
            m_iVersion = 0;
        }

        static CSerialize s_readAr;
        public static CSerialize   ReadStream(byte []buffer, int nBuffSize)
        {
            if (s_readAr == null)
                s_readAr = new CSerialize(SerializeType.read, buffer, nBuffSize);
            else
                s_readAr.ResetStream(SerializeType.read, buffer, nBuffSize);
            return s_readAr;
        }

        static CSerialize s_writeAr;
        public static CSerialize WriteStream()
        {
            if (s_writeAr == null)
                s_writeAr = new CSerialize(SerializeType.write);
            else
                s_writeAr.ResetStream(SerializeType.write);
            return s_writeAr;
        }

        ~CSerialize()
        {
            if (m_eSerializeType != SerializeType.read && !string.IsNullOrEmpty(m_sFileName))
            {
                Close();
            }
        }

        public void SetVersion(int nVersion)
        {
            m_iVersion = nVersion;
        }
        public int GetVersion()
        {
            return m_iVersion;
        }

        public void Close()
        {
            Flush();
            m_bCreate = false;
            m_eSerializeType = SerializeType.read;
            m_sFileName = string.Empty;
        }

        public void Flush()
        {
            if (string.IsNullOrEmpty(m_sFileName))
                return;

            // 写入文件
            if (m_eSerializeType != SerializeType.read)
            {
                try
                {
                    FileStream pFile = null;
                    if (!m_bCreate && m_eSerializeType == SerializeType.write)
                    {
                        m_bCreate = true;
                        if (File.Exists(m_sFileName))
                            File.Delete(m_sFileName);
                        pFile = File.Open(m_sFileName, FileMode.CreateNew, FileAccess.Write);
                    }
                    else
                    {
                        if (GetBufferSize() == 0)
                            return;
                        pFile = File.Open(m_sFileName, FileMode.Append, FileAccess.Write);
                        if (pFile != null)
                        {
                            pFile.Seek(0, SeekOrigin.End);
                        }
                    }

                    if (pFile != null)
                    {
                        if (GetBufferSize() > 0)
                            pFile.Write(GetBuffer(), 0, GetBufferSize());
                        pFile.Flush();
                        pFile.Close();
                        m_kFile.Flush();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public byte[] GetBuffer()
        {
            return m_kFile.GetBuffer();
        }
        public int GetBufferSize()
        {
            return m_kFile.GetBufferSize();
        }
        public bool IsLoading()
        {
            return m_eSerializeType == SerializeType.read;
        }

        public void Read(ref int nValue)
        {
            m_kFile.ReadInt(ref nValue);
        }
        public void Read(ref uint nValue)
        {
            m_kFile.ReadInt(ref nValue);
        }
        public void Read(ref long nValue)
        {
            m_kFile.ReadInt(ref nValue);
        }
        public void Read(ref ulong nValue)
        {
            m_kFile.ReadInt(ref nValue);
        }
        public void Read(ref byte yValue)
        {
            m_kFile.ReadInt(ref yValue);
        }
        public void Read(ref short wValue)
        {
            m_kFile.ReadInt(ref wValue);
        }
        public void Read(ref ushort wValue)
        {
            m_kFile.ReadInt(ref wValue);
        }
        public void Read(ref bool bValue)
        {
            m_kFile.ReadInt(ref bValue);
        }
        public void Read(ref float fValue)
        {
            m_kFile.ReadInt(ref fValue);
        }
        public void Read(ref double fValue)
        {
            m_kFile.ReadInt(ref fValue);
        }
        public void Read(ref string str)
        {
            m_kFile.ReadString(ref str);
        }
        public void ReadStringUTF32(ref string str, int nLen)
        {
            m_kFile.ReadStringUTF32(ref str, nLen);
        }
        public void ReadStringUTF8(ref string str, int nLen)
        {
            m_kFile.ReadStringUTF8(ref str, nLen);
        }
        public void Read(ref byte[] vArray)
        {
            m_kFile.ReadIntArray(ref vArray);
        }
        public void Read(ref int[] vArray)
        {
            m_kFile.ReadIntArray(ref vArray);
        }
        public void Read(ref uint[] vArray)
        {
            m_kFile.ReadIntArray(ref vArray);
        }
        public void Read(ref float[] vArray)
        {
            m_kFile.ReadIntArray(ref vArray);
        }
        public void Read(ref string[] vArray)
        {
            m_kFile.ReadIntArray(ref vArray);
        }
        public void Read(ref Vector3[] vArray)
        {
            m_kFile.ReadIntArray(ref vArray);
        }
        public void Read(ref Color32[] vArray)
        {
            m_kFile.ReadIntArray(ref vArray);
        }
        public void ReadArray(ref byte []vArray, int nLen)
        {
            m_kFile.ReadArray(ref vArray, nLen);
        }
        public void Read(ref Vector3 tValue)
        {
            if (tValue == null)
                tValue = Vector3.zero;
            m_kFile.ReadInt(ref tValue.x);
            m_kFile.ReadInt(ref tValue.y);
            m_kFile.ReadInt(ref tValue.z);
        }
        public void Read(ref Quaternion tValue)
        {
            if (tValue == null)
                tValue = new Quaternion();
            m_kFile.ReadInt(ref tValue.x);
            m_kFile.ReadInt(ref tValue.y);
            m_kFile.ReadInt(ref tValue.z);
            m_kFile.ReadInt(ref tValue.w);
        }
        public void Read(ref Rect tValue)
        {
            if (tValue == null)
                tValue = new Rect();
            float xMin = 0.0f, yMin = 0.0f, xMax = 0.0f, yMax = 0.0f;
            m_kFile.ReadInt(ref xMin);
            m_kFile.ReadInt(ref yMin);
            m_kFile.ReadInt(ref xMax);
            m_kFile.ReadInt(ref yMax);
            tValue.xMin = xMin;
            tValue.yMin = yMin;
            tValue.xMax = xMax;
            tValue.yMax = yMax;
        }
        public void Read(ref Bounds tValue)
        {
            if (tValue == null)
                tValue = new Bounds();
            Vector3 vMin = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 vMax = new Vector3(0.0f, 0.0f, 0.0f);
            Read(ref vMin);
            Read(ref vMax);
            tValue.SetMinMax(vMin, vMax);
        }
        public  void Read(ref Color32 tValue)
        {
            m_kFile.ReadInt(ref tValue);
        }

        public void Write(int nValue)
        {
            m_kFile.WriteInt(nValue);
        }
        public void Write(uint nValue)
        {
            m_kFile.WriteInt(nValue);
        }
        public void Write(long nValue)
        {
            m_kFile.WriteInt(nValue);
        }
        public void Write(ulong nValue)
        {
            m_kFile.WriteInt(nValue);
        }
        public void Write(byte yValue)
        {
            m_kFile.WriteInt(yValue);
        }
        public void Write(short wValue)
        {
            m_kFile.WriteInt(wValue);
        }
        public void Write(ushort wValue)
        {
            m_kFile.WriteInt(wValue);
        }
        public void Write(bool bValue)
        {
            m_kFile.WriteInt(bValue);
        }
        public void Write(float fValue)
        {
            m_kFile.WriteInt(fValue);
        }
        public void Write(double fValue)
        {
            m_kFile.WriteInt(fValue);
        }
        public void Write(string str)
        {
            m_kFile.WriteString(str);
        }
        public void PushTextString(string str)
        {
            m_kFile.PushTextString(str);
        }
        public void PushTextString(byte[] str)
        {
            m_kFile.PushTextString(str);
        }
        public void WriteStringUTF32(string str, int nSize)
        {
            m_kFile.WriteStringUTF32(str, nSize);
        }
        public void WriteStringUTF8(string str, int nSize)
        {
            m_kFile.WriteStringUTF8(str, nSize);
        }
        public void Write(byte[] vArray)
        {
            m_kFile.WriteIntArray(vArray);
        }
        public void Write(int[] vArray)
        {
            m_kFile.WriteIntArray(vArray);
        }
        public void Write(uint[] vArray)
        {
            m_kFile.WriteIntArray(vArray);
        }
        public void Write(float[] vArray)
        {
            m_kFile.WriteIntArray(vArray);
        }
        public void Write(string[] vArray)
        {
            m_kFile.WriteIntArray(vArray);
        }
        public void Write(Vector3[] vArray)
        {
            m_kFile.WriteIntArray(vArray);
        }
        public void Write(Color32 []vArray)
        {
            m_kFile.WriteIntArray(vArray);
        }
        public void WriteArray(byte []vArray, int nLen)
        {
            m_kFile.WriteArray(vArray, nLen);
        }
        public void Write(Vector3 v)
        {
            m_kFile.WriteInt(v.x);
            m_kFile.WriteInt(v.y);
            m_kFile.WriteInt(v.z);
        }
        public void Write(Quaternion v)
        {
            m_kFile.WriteInt(v.x);
            m_kFile.WriteInt(v.y);
            m_kFile.WriteInt(v.z);
            m_kFile.WriteInt(v.w);
        }
        public void Write(Rect tValue)
        {
            m_kFile.WriteInt(tValue.xMin);
            m_kFile.WriteInt(tValue.yMin);
            m_kFile.WriteInt(tValue.xMax);
            m_kFile.WriteInt(tValue.yMax);
        }
        public void Write(Bounds v)
        {
            Write(v.min);
            Write(v.max);
        }
        public void Write(Color32 v)
        {
            m_kFile.WriteInt(v);
        }

        //-----------------------------------------------
        public byte ReadByte(byte yDef = 0)
        {
            m_kFile.ReadInt(ref yDef);
            return yDef;
        }
        public bool ReadBool(bool bDef = false)
        {
            m_kFile.ReadInt(ref bDef);
            return bDef;
        }
        public short ReadInt16(short nDef = 0)
        {
            m_kFile.ReadInt(ref nDef);
            return nDef;
        }
        public int ReadInt32(int nDef = 0)
        {
            m_kFile.ReadInt(ref nDef);
            return nDef;
        }
        public float ReadFloat(float fDef = 0.0f)
        {
            m_kFile.ReadInt(ref fDef);
            return fDef;
        }
        public string ReadString(string szDef = "")
        {
            m_kFile.ReadString(ref szDef);
            return szDef;
        }
        //-----------------------------------------------
        public void ReadWriteValue(ref bool tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref byte tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref short tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref ushort tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref int tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref uint tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref long tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref ulong tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref float tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref double tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref string tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref byte[] tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref int[] tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref uint[] tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref float[] tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref string[] tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref Vector3[] tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref Color32[] tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref Vector3 tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref Quaternion tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }    
        public void ReadWriteValue(ref Rect tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref Bounds tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteValue(ref Color32 tValue)
        {
            if (IsLoading())
                Read(ref tValue);
            else
                Write(tValue);
        }
        public void ReadWriteStringUTF32(ref string tValue, int nSize)
        {
            if (IsLoading())
                ReadStringUTF32(ref tValue, nSize);
            else
                WriteStringUTF32(tValue, nSize);
        }
        public void ReadWriteStringUTF8(ref string tValue, int nSize)
        {
            if (IsLoading())
                ReadStringUTF8(ref tValue, nSize);
            else
                WriteStringUTF8(tValue, nSize);
        }
        public void ReadWriteArray(ref byte []tValue, int nSize)
        {
            if (IsLoading())
                ReadArray(ref tValue, nSize);
            else
                WriteArray(tValue, nSize);
        }

        public delegate void SerializeValue<_Ty>(CSerialize ar, ref _Ty tValue);
        public void ReadWriteValue<_Ty>(ref _Ty tValue, SerializeValue<_Ty> func) where _Ty:new ()
        {
            if (tValue == null)
                tValue = new _Ty();
            func(this, ref tValue);
        }

        public void ReadWriteValue<_Ty>(ref _Ty tValue) where _Ty : ISerializable,new()
        {
            if (tValue == null)
                tValue = new _Ty();
            tValue.Serialize(this);
        }

        public void AutoNewValue<_Ty>(ref _Ty tValue) where _Ty : new()
        {
            if (tValue == null)
                tValue = new _Ty();
        }

        public void SerializeArray<_Ty>(ref _Ty[] aValue)
        {
            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                if (nLen <= 0)
                    aValue = new _Ty[0];
                else
                {
                    aValue = new _Ty[nLen];
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Length : 0;
                Write(nLen);
            }
        }

        public void SerializeBaseArray(ref List<string> aValue)
        {
            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                if (aValue == null)
                {
                    aValue = new List<string>();
                }
                aValue.Clear();
                for (int i = 0; i < nLen; ++i)
                {
                    string tmpStr = string.Empty;
                    this.Read(ref tmpStr);
                    aValue.Add(tmpStr);
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Count : 0;
                Write(nLen);
                for (int i=0;i<nLen;++i)
                {
                    this.Write(aValue[i]);
                }
            }
        }

        public void SerializeBaseArray(ref List<int> aValue)
        {
            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                if (aValue == null)
                {
                    aValue = new List<int>();
                }
                aValue.Clear();
                for (int i = 0; i < nLen; ++i)
                {
                    int tmp = 0;
                    this.Read(ref tmp);
                    aValue.Add(tmp);
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Count : 0;
                Write(nLen);
                for (int i=0;i<nLen;++i) {
                    this.Write(aValue[i]);
                }
            }
        }



        public delegate void SerializeArrayNode<_Ty>(CSerialize ar, ref _Ty value);
        public void SerializeArray<_Ty>(ref _Ty[] aValue, SerializeArrayNode<_Ty> serializeFunc)
        {
            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                if (nLen <= 0)
                    aValue = new _Ty[0];
                else
                {
                    aValue = new _Ty[nLen];
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Length : 0;
                Write(nLen);
            }
            for (int i = 0; i < nLen; ++i)
            {
                serializeFunc(this, ref aValue[i]);
            }
        }

        // 功能：读取一个对象数组
        public void SerializeObjectArray<_Ty>(ref _Ty[] aValue) where _Ty : ISerializable, new()
        {
            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                if (nLen > 0)
                    aValue = new _Ty[nLen];
                else
                    aValue = null;
                for (int i = 0; i < nLen; ++i)
                {
                    aValue[i] = new _Ty();
                    aValue[i].Serialize(this);
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Length : 0;
                Write(nLen);
                for (int i = 0; i < nLen; ++i)
                {
                    _Ty value = aValue[i];
                    if (value == null)
                        value = new _Ty();
                    value.Serialize(this);
                }
            }
        }

        public void SerializeArray<_Ty>(ref List<_Ty> aValue, SerializeArrayNode<_Ty> serializeFunc) where _Ty : new()
        {
            if (aValue == null)
                aValue = new List<_Ty>();

            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                aValue.Clear();
                for (int i = 0; i < nLen; ++i)
                {
                    _Ty value = new _Ty();
                    serializeFunc(this, ref value);
                    if (value != null)
                    {
                        aValue.Add(value);
                    }
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Count : 0;
                Write(nLen);
                for (int i = 0; i < nLen; ++i)
                {
                    _Ty value = aValue[i];
                    serializeFunc(this, ref value);
                }
            }
        }

        
        public void SerializeStructArray<_Ty>(ref List<_Ty> aValue) where _Ty : ISerializable, new()
        {
            if (aValue == null)
                aValue = new List<_Ty>();
            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                aValue.Clear();
                for (int i = 0; i < nLen; ++i)
                {
                    _Ty value = new _Ty();
                    value.Serialize(this);
                    aValue.Add(value);
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Count : 0;
                Write(nLen);
                for (int i = 0; i < nLen; ++i)
                {
                    _Ty value = aValue[i];
                    value.Serialize(this);
                }
            }
        }
        
        public delegate void SerializeIterator<_TyKey, _TyValue>(CSerialize ar, ref _TyKey key, ref _TyValue value);
        public void SerializeDictionary<_TyKey, _TyValue>(ref Dictionary<_TyKey, _TyValue> aValue, SerializeIterator<_TyKey, _TyValue> serializeFunc)
        {
            if (aValue == null)
                aValue = new Dictionary<_TyKey, _TyValue>();

            int nLen = 0;
            if (IsLoading())
            {
                Read(ref nLen);
                if (nLen < 0 || nLen > 1024 * 1024)
                    nLen = 0;
                if (aValue == null)
                    aValue = new Dictionary<_TyKey, _TyValue>();
                aValue.Clear();
                for (int i = 0; i < nLen; ++i)
                {
                    _TyKey key = default(_TyKey);
                    _TyValue value = default(_TyValue);
                    serializeFunc(this, ref key, ref value);
                    if( key != null && value != null )
                        aValue[key] = value;
                }
            }
            else
            {
                nLen = aValue != null ? aValue.Count : 0;
                Write(nLen);
                if (nLen > 0)
                {
                    Dictionary<_TyKey, _TyValue>.Enumerator it = aValue.GetEnumerator();
                    while (it.MoveNext())
                    {
                        _TyKey key = it.Current.Key;
                        _TyValue value = it.Current.Value;
                        serializeFunc(this, ref key, ref value);
                    }
                }
            }
        }
    }

}


