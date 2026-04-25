using System.Xml.Serialization;

public readonly record struct GitInfo(string GitUrl, string FolderName, string CommitSha);
