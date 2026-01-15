namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal static class Helpers
    {
        public const string NAMESPACE = "EncosyTower.UserDataVaults";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string VAULT_ATTRIBUTE = $"global::{NAMESPACE}.UserDataVaultAttribute";
        public const string ACCESS_ATTRIBUTE = $"global::{NAMESPACE}.UserDataAccessAttribute";
        public const string IUSER_DATA = $"global::{NAMESPACE}.IUserData";
        public const string IUSER_DATA_ACCESS = $"global::{NAMESPACE}.IUserDataAccess";
        public const string USER_DATA_STORAGE_BASE = $"global::{NAMESPACE}.UserDataStorageBase<";
        public const string ENCRYPTION_BASE = "EncryptionBase";
        public const string STRING_VAULT = "StringVault";
        public const string ILOGGER = "ILogger";
        public const string TASK_ARRAY_POOL = "global::System.Buffers.ArrayPool<UnityTask>";
        public const string STORAGE_ARGS = "UserDataStorageArgs";
        public const string COMPLETED_TASK = "UnityTasks.GetCompleted()";
        public const string WHEN_ALL_TASKS = "UnityTasks.WhenAll(tasks)";
        public const string NOT_NULL = "[NotNull]";
        public const string STRING_ID = "StringIdᐸstringᐳ";
        public const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string GENERATOR = "\"EncosyTower.SourceGen.Generators.UserDataVaults.UserDataVaultGenerator\"";
        public const string HIDE_IN_CALL_STACK = "[HideInCallstack, StackTraceHidden]";
    }
}
