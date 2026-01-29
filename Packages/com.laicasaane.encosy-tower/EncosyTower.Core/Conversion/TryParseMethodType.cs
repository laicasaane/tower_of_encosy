namespace EncosyTower.Conversion
{
    /// <summary>
    /// Specifies the type of TryParse method to use for parsing.
    /// The method must have the same signature as <see cref="ITryParse{T}"/>
    /// or <see cref="ITryParseSpan{T}"/>.
    /// </summary>
    public enum TryParseMethodType : byte
    {
        None = 0,
        Instance,
        Static,
    }
}
