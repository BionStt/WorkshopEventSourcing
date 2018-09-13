﻿using System;

namespace PublicClassifiedAdScreen.Tools
{
    public interface ISerializer
    {
        bool IsJsonSerializer { get; }

        byte[] Serialize(object obj);
        object Deserialize(byte[] data, Type type);
    }
}
