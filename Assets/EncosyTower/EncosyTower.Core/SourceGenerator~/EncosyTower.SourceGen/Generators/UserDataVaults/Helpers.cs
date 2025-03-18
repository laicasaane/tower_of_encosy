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
        public const string NOT_NULL = "[global::System.Diagnostics.CodeAnalysis.NotNull]";
        public const string LOG_ERROR = "global::EncosyTower.Logging.DevLoggerAPI.LogError";
        public const string STRING_ID = "global::EncosyTower.StringIds.StringId<string>";
        public const string STRING_ID_MAKE = "global::EncosyTower.StringIds.StringToId.MakeFromManaged";
    }
}
