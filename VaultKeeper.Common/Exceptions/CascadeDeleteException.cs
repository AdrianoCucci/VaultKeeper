using System;
using System.Collections.Generic;

namespace VaultKeeper.Common.Exceptions;

public class CascadeDeleteException<TParent, TChild>(string? message = null, Exception? innerException = null) : Exception(message, innerException)
{
    public required TParent Parent { get; init; }
    public required IEnumerable<TChild> ConflictingChildren { get; init; }
}
