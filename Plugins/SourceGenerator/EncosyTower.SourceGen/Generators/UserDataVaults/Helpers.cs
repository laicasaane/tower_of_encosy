namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal static class Helpers
    {
        public const string NAMESPACE = "EncosyTower.UserDataVaults";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string ATTRIBUTE = $"global::{NAMESPACE}.UserDataVaultAttribute";
        public const string IUSER_DATA = $"global::{NAMESPACE}.IUserData";
        public const string IUSER_DATA_ACCESS = $"global::{NAMESPACE}.IUserDataAccess";
        public const string USER_DATA_STORAGE_BASE = $"global::{NAMESPACE}.UserDataStorageBase<";
        public const string ENCRYPTION_BASE = "global::EncosyTower.Encryption.EncryptionBase";
        public const string ILOGGER = "global::EncosyTower.Logging.ILogger";
        public const string TASK_ARRAY_POOL = "global::System.Buffers.ArrayPool<UnityTask>";
        public const string STORAGE_ARGS = "global::EncosyTower.UserDataVaults.UserDataStorageArgs";
        public const string COMPLETED_TASK = "UnityTasks.GetCompleted()";
        public const string WHEN_ALL_TASKS = "UnityTasks.WhenAll(tasks)";
        public const string NOT_NULL = "[NotNull]";
        public const string STRING_ID = "global::EncosyTower.StringIds.StringId<string>";
        public const string STRING_ID_GET = "global::EncosyTower.StringIds.StringToId.Get";
        public const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string GENERATOR = "\"EncosyTower.SourceGen.Generators.UserDataVaults.UserDataVaultGenerator\"";
    }
}
