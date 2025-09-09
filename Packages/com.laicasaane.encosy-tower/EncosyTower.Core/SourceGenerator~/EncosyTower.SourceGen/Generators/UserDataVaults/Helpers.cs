namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal static class Helpers
    {
        public const string NAMESPACE = "EncosyTower.UserDataVaults";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string ATTRIBUTE = $"global::{NAMESPACE}.UserDataVaultAttribute";
        public const string IUSER_DATA_ACCESS = $"global::{NAMESPACE}.IUserDataAccess";
        public const string USER_DATA_STORAGE_BASE = $"global::{NAMESPACE}.UserDataStorageBase<";
        public const string ENCRYPTION_BASE = "global::EncosyTower.Encryption.EncryptionBase";
        public const string ILOGGER = "global::EncosyTower.Logging.ILogger";
        public const string TASK_ARRAY_POOL = "global::System.Buffers.ArrayPool<UnityTask>";
        public const string COMPLETED_TASK = "global::EncosyTower.Tasks.UnityTasks.GetCompleted()";
        public const string WHEN_ALL_TASKS = "global::EncosyTower.Tasks.UnityTasks.WhenAll(tasks)";
        public const string NOT_NULL = "[global::System.Diagnostics.CodeAnalysis.NotNull]";
        public const string STRING_ID = "global::EncosyTower.StringIds.StringId<string>";
        public const string STRING_ID_MAKE = "global::EncosyTower.StringIds.StringToId.MakeFromManaged";
        public const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators.UserDataVaults.UserDataVaultGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
    }
}
