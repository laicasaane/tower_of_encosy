using Latios;
using Latios.Authoring;
using Unity.Entities;
using UnityEngine.Scripting;

namespace Module.EntitySystems
{
    [Preserve]
    internal sealed class GameBakingBootstrap : ICustomBakingBootstrap
    {
        public void InitializeBakingForAllWorlds(ref CustomBakingBootstrapContext context)
        {
            Latios.Authoring.CoreBakingBootstrap.ForceRemoveLinkedEntityGroupsOfLength1(ref context);
            Latios.Transforms.Authoring.TransformsBakingBootstrap.InstallLatiosTransformsBakers(ref context);
            Latios.Psyshock.Authoring.PsyshockBakingBootstrap.InstallUnityColliderBakers(ref context);
            Latios.Kinemation.Authoring.KinemationBakingBootstrap.InstallKinemation(ref context);
            Latios.Unika.Authoring.UnikaBakingBootstrap.InstallUnikaEntitySerialization(ref context);
        }
    }

    [Preserve]
    internal sealed class GameEditorBootstrap : ICustomEditorBootstrap
    {
        public World Initialize(string defaultEditorWorldName)
        {
            var world = new LatiosWorld(defaultEditorWorldName, WorldFlags.Editor) {
                useExplicitSystemOrdering = true
            };

            var systems = DefaultWorldInitialization.GetAllSystemTypeIndices(WorldSystemFilterFlags.Default, true);
            BootstrapTools.InjectUnitySystems(systems, world, world.simulationSystemGroup);

            Latios.Transforms.TransformsBootstrap.InstallTransforms(world);
            Latios.Kinemation.KinemationBootstrap.InstallKinemation(world);
            Latios.Calligraphics.CalligraphicsBootstrap.InstallCalligraphics(world);

            BootstrapTools.InjectRootSuperSystems(systems, world, world.simulationSystemGroup);

            return world;
        }
    }

    [Preserve]
    internal sealed class GameBootstrap : ICustomBootstrap
    {
        public bool Initialize(string defaultWorldName)
        {
            var world = new LatiosWorld(defaultWorldName) {
                useExplicitSystemOrdering = true
            };

            World.DefaultGameObjectInjectionWorld = world;

            var systems = DefaultWorldInitialization.GetAllSystemTypeIndices(WorldSystemFilterFlags.Default);

            BootstrapTools.InjectUnitySystems(systems, world, world.simulationSystemGroup);

            //Latios.CoreBootstrap.InstallSceneManager(world);
            Latios.Transforms.TransformsBootstrap.InstallTransforms(world);
            Latios.Kinemation.KinemationBootstrap.InstallKinemation(world);
            Latios.Calligraphics.CalligraphicsBootstrap.InstallCalligraphics(world);
            Latios.Unika.UnikaBootstrap.InstallUnikaEntitySerialization(world);
            Latios.LifeFX.LifeFXBootstrap.InstallLifeFX(world);

            BootstrapTools.InjectRootSuperSystems(systems, world, world.simulationSystemGroup);

            world.initializationSystemGroup.SortSystems();
            world.simulationSystemGroup.SortSystems();
            world.presentationSystemGroup.SortSystems();

            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
            return true;
        }
    }
}
