using System;

namespace EncosyTower.Databases.Authoring.SourceGen
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedSheetContainerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedSheetAttribute : Attribute
    {
        public Type IdType { get; }

        public Type DataType { get; }

        public Type DataTableAssetType { get; }

        public string DataTableAssetName { get; }

        public GeneratedSheetAttribute(Type idType, Type dataType, Type dataTableAssetType, string tableAssetName)
        {
            IdType = idType;
            DataType = dataType;
            DataTableAssetType = dataTableAssetType;
            DataTableAssetName = tableAssetName;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedSheetRowAttribute : Attribute
    {
        public Type IdType { get; }

        public Type DataType { get; }

        public GeneratedSheetRowAttribute(Type idType, Type dataType)
        {
            IdType = idType;
            DataType = dataType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedDataRowAttribute : Attribute
    {
        public Type DataType { get; }

        public GeneratedDataRowAttribute(Type dataType)
        {
            DataType = dataType;
        }
    }
}
