using System.Xml.Serialization;

namespace PluginsIntegrator.Slnx;

[XmlRoot(ElementName = "Platform")]
public class Platform
{

    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; }
}

[XmlRoot(ElementName = "Configurations")]
public class Configurations
{

    [XmlElement(ElementName = "Platform")]
    public List<Platform> Platform { get; set; }
}

[XmlRoot(ElementName = "Project")]
public class Project
{

    [XmlAttribute(AttributeName = "Path")]
    public string Path { get; set; }
}

[XmlRoot(ElementName = "Solution")]
public class Solution
{

    [XmlElement(ElementName = "Configurations")]
    public Configurations Configurations { get; set; }

    [XmlElement(ElementName = "Project")]
    public List<Project> Projects { get; set; }
}
