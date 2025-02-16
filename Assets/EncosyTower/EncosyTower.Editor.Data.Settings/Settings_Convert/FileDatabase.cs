using EncosyTower.Data;
using EncosyTower.Data.Authoring;
using UnityEngine;

namespace EncosyTower.Editor.Data.Settings
{
    [HideInInspector]
    [Database(NamingStrategy.SnakeCase)]
    internal partial class FileDatabase
    {
        [Table] public FileTableAsset fileList;
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
