using System;
using Cysharp.Threading.Tasks;
using EncosyTower.Modules.Encryption;
using EncosyTower.Modules.NameKeys;

namespace EncosyTower.Modules.UserDataStores
{
    public interface IUserDataAccess { }

    public interface IUserData
    {
        string UserId { get; set; }

        string Version { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class UserDataAccessProviderAttribute : Attribute
    {
        public string Prefix { get; set; }

        public string Suffix { get; set; }
    }

    public abstract partial class UserDataBase : IUserData
    {
        public string UserId { get; set; }

        public string Version { get; set; }
    }

    public sealed class UserDataStorage<T> where T : IUserData, new()
    {
        public UserDataStorage(NameKey<string> key, EncryptionBase encryption) { }

        public string UserId { get; set; }

        public void SetUserData(string userId, string version) { }

        public T Data => default;

        public bool IsDataValid => Data != null;

        public void Initialize() { }

        public void MarkDirty(bool isDirty = true) { }

        public void CreateData() { }

        public UniTask LoadFromFirestore() => default;

        public T GetDataFromFirestore() => default;

        public T GetDataFromDevice() => default;

        public void SetToDevice(T data) { }

        public UniTask LoadFromDevice() => default;

        public void SaveToDevice() { }

        public UniTask SaveToFirestoreAsync() => default;

        public void Save(bool forceToFirestore) { }

        public UniTask SaveAsync(bool forceToFirestore) => default;

        public void DeepCloneDataFromFirestoreToDevice() { }
    }
}