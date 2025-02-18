using EncosyTower.Data;
using EncosyTower.Databases.Authoring;
using EncosyTower.Naming;
using UnityEngine;

namespace EncosyTower.Databases.Settings
{
    [HideInInspector]
    [Database(NamingStrategy.SnakeCase)]
    internal readonly partial struct FileDatabase
    {
        [Table] public FileTableAsset FileList { get; }
    }

    internal sealed partial class FileTableAsset : DataTableAsset<int, FileData>, IDataTableAsset { }

    internal partial struct FileData : IData
    {
        [DataProperty]
        public readonly int Id => Get_Id();

        [DataProperty]
        public readonly string FileName => Get_FileName();

        [DataProperty]
        public readonly string FileId => Get_FileId();

        [DataProperty]
        public readonly string MimeType => Get_MimeType();

        [DataProperty]
        public readonly string Url => Get_Url();
    }
}
