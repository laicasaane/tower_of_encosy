using System;
using EncosyTower.Naming;

namespace EncosyTower.Databases.Authoring.SourceGen
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TableNamingAttribute : Attribute
    {
        public string SheetName { get; }

        public NameCasing NameCasing { get; }

        public TableNamingAttribute(string sheetName)
        {
            this.SheetName = sheetName;
        }

        public TableNamingAttribute(string sheetName, NameCasing nameCasing)
        {
            this.SheetName = sheetName;
            this.NameCasing = nameCasing;
        }
    }
}
