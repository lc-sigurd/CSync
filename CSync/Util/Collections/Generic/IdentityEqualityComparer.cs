/*
 * This file is largely based upon
 * https://github.com/lc-sigurd/sigurd/blob/90b885a2612fb4667bcf4b882d128cea6038976b/SigurdLib.Util/Collections/Generic/IdentityEqualityComparer.cs
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team license this file to themselves under the LGPL-3.0-or-later license.
 *
 * Copyright (C) 2024 Sigurd Team
 * The Sigurd Team license this file to you under the CC-BY-NC-SA-4.0 license.
 */

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSync.Util.Collections.Generic;

// https://stackoverflow.com/a/8946825
public sealed class IdentityEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public int GetHashCode(T value) => RuntimeHelpers.GetHashCode(value);

    public bool Equals(T left, T right) => ReferenceEquals(left, right);
}
