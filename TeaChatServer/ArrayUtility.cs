using System;

namespace TeaChat.Uitlity
{
    class ArrayUtility
    {
        /// <summary>
        /// Copy source byte array form start bit to (end-1) bit to destination array 
        /// starting from specified offset form 0 to at most capacity of desination array.
        /// </summary>
        /// <param name="dst">copy destination</param>
        /// <param name="dst_offset">index offset from 0</param>
        /// <param name="src">copy source</param>
        /// <param name="src_start">copy start bit index</param>
        /// <param name="src_end">copy end bit index</param>
        /// <returns>non-negative copied data size for success or -1 as error</returns>
        public static int CopyByteArray(byte[] dst, int dst_offset, byte[] src, int src_start, int src_end)
        {
            if (dst == null || src == null) throw new ArgumentNullException();

            int copy_size;
            int dst_end = dst.Length;
            int dst_index, src_index;

            if (src_end > src.Length) src_end = src.Length;

            for (
                dst_index = dst_offset, src_index = src_start, copy_size = 0;
                dst_index < dst_end && src_index < src_end;
                dst_index++, src_index++, copy_size++
                )
            {
                dst[dst_index] = src[src_index];
            }

            return copy_size;
        }

        public static int ZeroByteArray(byte[] buff)
        {
            if (buff == null) throw new ArgumentNullException();

            int index = 0;

            for (index = 0; index < buff.Length; index++)
            {
                buff[index] = 0x00;
            }

            return index;
        }
    }
}
