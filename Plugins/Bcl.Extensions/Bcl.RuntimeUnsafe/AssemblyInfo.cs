using System.Security.Permissions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EncosyTower.Core")]
[assembly: InternalsVisibleTo("EncosyTower.Editor")]
[assembly: IgnoresAccessChecksTo("mscorlib")]
[assembly: IgnoresAccessChecksTo("System.Private.CoreLib")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
