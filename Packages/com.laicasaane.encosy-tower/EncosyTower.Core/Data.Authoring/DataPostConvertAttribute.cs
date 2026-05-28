using System;
using EncosyTower.Core;

namespace EncosyTower.Data.Authoring
{
    /// <summary>
    /// <para>
    /// Informs the authoring pipeline that the annotated property or field should be assigned by
    /// the post-convert mechanism provided by <see cref="EncosyTower.Databases.Authoring.IDataSheet"/>.
    /// </para>
    /// <para>
    /// To participate in the post-convert mechanism, start with implementing the partial method <c>OnPostConvert</c>
    /// on the authoring sheet derived from <see cref="Cathei.BakingSheet.SheetRow{TKey}"/>.
    /// </para>
    /// <para>
    /// Whenever the authoring sheet needs more preparation before post-convert,
    /// implement the partial method <c>OnInitialize</c> on that type.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The generated property will be annotated with <see cref="Cathei.BakingSheet.NonSerializedAttribute"/>,
    /// and its name will be suffixed with <c>_PostConvert</c>.
    /// </remarks>
    [ApiForAuthoring]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DataPostConvertAttribute : Attribute
    {
    }
}
