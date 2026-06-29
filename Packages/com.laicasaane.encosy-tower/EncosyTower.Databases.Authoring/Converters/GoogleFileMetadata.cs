using System;

namespace EncosyTower.Databases.Authoring
{
    public readonly record struct GoogleFileMetadata(string Name, DateTimeOffset ModifiedTime);
}
