﻿using FishNet.Utility.Performance;
using System;

namespace FishyFacepunch
{
    internal struct LocalPacket
    {
        public byte[] Data;
        public int Length;
        public byte Channel;
        public LocalPacket(ArraySegment<byte> data, byte channel)
        {
            Data = ByteArrayPool.Retrieve(data.Count);
            Length = data.Count;
            Buffer.BlockCopy(data.Array, data.Offset, Data, 0, Length);
            Channel = channel;
        }

        public void Dispose()
        {
            if (Data != null)
                ByteArrayPool.Store(Data);
        }
    }
}
