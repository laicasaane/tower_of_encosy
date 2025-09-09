using System;
using EncosyTower.Naming;

namespace EncosyTower.Databases.Authoring.SourceGen
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TableNamingAttribute : Attribute
    {
        public string SheetName { get; }

        public NamingStrategy NamingStrategy { get; }

        public TableNamingAttribute(string sheetName)
        {
            this.SheetName = sheetName;
        }

        public TableNamingAttribute(string sheetName, NamingStrategy namingStrategy)
        {
            this.SheetName = sheetName;
            this.NamingStrategy = namingStrategy;
        }
    }
}
