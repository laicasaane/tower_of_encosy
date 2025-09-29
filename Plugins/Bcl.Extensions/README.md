# Bcl.CollectionsMarshal
Provides an implementation of `System.Runtime.InteropServices.CollectionsMarshal` for .NET Standard 2.0 and 2.1.

# How does it work?
The `Bcl.CollectionsMarshal` library is compiled against a custom `netstandard` assembly that exposes the `_items` and `_size` fields of `List<T>` publicly. When combined with `IgnoresAccessChecksToAttribute` and `SecurityPermissionAttribute` with `SkipVerification = true`, the assembly can access these fields without runtime errors.

# Disclaimer
This library is provided with no promise of support or stability. If you choose to use it, you do so at your own risk.
