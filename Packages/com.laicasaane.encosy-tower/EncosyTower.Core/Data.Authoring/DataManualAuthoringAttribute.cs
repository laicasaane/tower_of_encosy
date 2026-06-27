using System;
using System.Diagnostics;
using EncosyTower.Core;

namespace EncosyTower.Data.Authoring
{
    /// <summary>
    /// <para>
    /// Informs the authoring pipeline that the annotated property or field should be assigned by
    /// a manual authoring mechanism which hooks into callbacks provided by
    /// <see cref="EncosyTower.Databases.Authoring.IDataSheet"/>.
    /// </para>
    /// <para>
    /// To hook into <c>IDataSheet</c> callbacks, implement these partial methods on the authoring sheet
    /// derived from <see cref="Cathei.BakingSheet.SheetRow{TKey}"/>:
    /// <list type="bullet">
    /// <item>OnPreprocess</item>
    /// <item>OnProcess</item>
    /// <item>OnPostprocess</item>
    /// </list>
    /// (Note: It is not required to implement all of them, it's best to choose only methods that make sense.)
    /// </para>
    /// </summary>
    /// <remarks>
    /// The generated property will be annotated with <see cref="Cathei.BakingSheet.NonSerializedAttribute"/>,
    /// and its name will be suffixed with <c>_Manual</c>.
    /// </remarks>
    [ApiForAuthoring]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR"), Conditional("ENCOSY_INCLUDE_AUTHORING")]
    public sealed class DataManualAuthoringAttribute : Attribute
    {
        public DataManualAuthoringAttribute(Type authoringType = null)
        {
            BackingType = authoringType;
        }

        public Type BackingType { get; }
    }
}
