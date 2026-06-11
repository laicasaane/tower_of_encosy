using System.Collections;
using EncosyTower.Types;

namespace Samples.RuntimeTypeCaches
{
    public partial class Tests
    {
        public void DoSomethingZ()
        {
            const string ASSEMBLY = "Samples.RuntimeTypeCaches";
            RuntimeTypeCache.GetTypesDerivedFrom<IEqualityComparer>(ASSEMBLY);
            RuntimeTypeCache.GetFieldsWithAttribute<SomeAttribute>();
            RuntimeTypeCache.GetInfo<SpecialAttribute>();
        }
    }
}
