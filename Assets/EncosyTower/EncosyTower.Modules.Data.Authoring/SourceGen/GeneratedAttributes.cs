using System;

namespace EncosyTower.Modules.Data.Authoring.SourceGen
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedSheetContainerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedSheetAttribute : Attribute
    {
        public Type IdType { get; }

        public Type DataType { get; }

        public Type DataTableAssetType { get; }

        public GeneratedSheetAttribute(Type idType, Type dataType, Type dataTableAssetType)
        {
            this.IdType = idType;
            this.DataType = dataType;
            this.DataTableAssetType = dataTableAssetType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedSheetRowAttribute : Attribute
    {
        public Type IdType { get; }

        public Type DataType { get; }

        public GeneratedSheetRowAttribute(Type idType, Type dataType)
        {
            this.IdType = idType;
            this.DataType = dataType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratedDataRowAttribute : Attribute
    {
        public Type DataType { get; }

        public GeneratedDataRowAttribute(Type dataType)
        {
            this.DataType = dataType;
        }
    }
}