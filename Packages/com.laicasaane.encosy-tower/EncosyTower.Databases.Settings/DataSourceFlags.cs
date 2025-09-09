using System;

namespace EncosyTower.Databases.Settings
{
    [Flags]
    public enum DataSourceFlags
    {
        None = 0,
        GoogleSheet = 1 << 0,
        Csv = 1 << 1,
        Excel = 1 << 2,
    }
}
