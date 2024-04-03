using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Netcode;

internal class ListierNetworkList<T> : NetworkList<T>, IList<T> where T : unmanaged, IEquatable<T>
{
    IEnumerator IEnumerable.GetEnumerator() => base.GetEnumerator();

    public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();

    public bool IsReadOnly => false;
}
