﻿namespace Stateless; 

internal static class ArrayHelper {

    public static T[] Empty<T>() {
#if NETSTANDARD1_0
        return new T[0];
#else
        return System.Array.Empty<T>();
#endif
    }

}