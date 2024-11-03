using System;

namespace EncosyTower.Modules.Data.Authoring.SourceGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SheetNamingAttribute : Attribute
    {
        public string SheetName { get; }

        public NamingStrategy NamingStrategy { get; }

        public SheetNamingAttribute(string sheetName)
        {
            this.SheetName = sheetName;
        }

        public SheetNamingAttribute(string sheetName, NamingStrategy namingStrategy)
        {
            this.SheetName = sheetName;
            this.NamingStrategy = namingStrategy;
        }
    }
}
