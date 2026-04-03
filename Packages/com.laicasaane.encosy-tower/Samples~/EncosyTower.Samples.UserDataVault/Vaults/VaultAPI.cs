using System;
using System.IO;
using System.Runtime.CompilerServices;
using EncosyTower.IO;
using EncosyTower.Serialization.NewtonsoftJson;
using EncosyTower.UserDataVaults;
using UnityEngine;

namespace EncosyTower.Samples.UserDataVault.Vaults;

internal static partial class VaultAPI
{
    private const string SAVE_FOLDER_NAME = "EncosyTower.Samples.UserDataVault.SaveFiles";
    private const string SAVE_FILE_EXTENSION = "json";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UserDataStoreArgs GetStoreArgs<TData, TStore>(Func<TData> createFunc)
        where TData : IUserData
        where TStore : UserDataStoreBase<TData>
        => new UserDataStoreDefault<TData>.Args(createFunc, new UserDataSourceDevice<TData>.Args(
              RootPath: GetDeviceSaveFolderPath()
            , SerializeFunc: Serialize
            , DeserializeFunc: Deserialize
            , FileExtension: SAVE_FILE_EXTENSION
            , MakeFilePathFunc: MakeFilePathFunc
        ));

    public static bool SaveFileExists(string userId, string fileName)
    {
        var rootPath = GetDeviceSaveFolderPath();
        var filePath = MakeFilePathFunc(rootPath, userId, fileName, SAVE_FILE_EXTENSION, null);
        return File.Exists(filePath);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDeviceSaveFolderPath()
        => GetDeviceSavePath(SAVE_FOLDER_NAME);

    public static void OpenDeviceSaveFolder()
    {
        var path = GetDeviceSaveFolderPath();

#if UNITY_EDITOR || UNITY_STANDALONE
        Application.OpenURL(path);
#else
        UnityEngine.Debug.Log(path);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Serialize<TData>(TData data, out string json)
        => JsonHelper.TrySerialize(data, out json);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Deserialize<TData>(string json, out TData data)
        => JsonHelper.TryDeserialize(json, out data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetDeviceSavePath(string folderName)
        => Path.GetFullPath(Path.Combine(
#if UNITY_EDITOR
              Application.dataPath, ".."
#else
              Application.persistentDataPath
#endif
            , folderName
        ));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MakeFilePathFunc(
          RootPath rootPath
        , string subFolderName
        , string fileName
        , string fileExtension
        , Type userDataType
    )
    {
        return Path.GetFullPath(Path.Combine(
              rootPath.Root
            , $"{subFolderName}_{fileName}.{fileExtension}"
        ));
    }
}
