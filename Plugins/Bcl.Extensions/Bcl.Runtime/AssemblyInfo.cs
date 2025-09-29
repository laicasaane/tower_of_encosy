using System.Security.Permissions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bcl.CollectionsMarshal")]
[assembly: InternalsVisibleTo("Bcl.CollectionsUnsafe")]
[assembly: IgnoresAccessChecksTo("mscorlib")]
[assembly: IgnoresAccessChecksTo("System.Private.CoreLib")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
