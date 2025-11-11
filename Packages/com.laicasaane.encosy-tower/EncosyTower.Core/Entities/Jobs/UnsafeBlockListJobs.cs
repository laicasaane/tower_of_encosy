#if UNITY_ENTITIES && LATIOS_FRAMEWORK

using EncosyTower.Collections;
using Latios;
using Latios.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
    [BurstCompile]
    public partial struct ClearUnsafeParallelBlockListJob : IJob
    {
        public UnsafeParallelBlockList list;

        [BurstCompile]
        public void Execute()
        {
            list.Clear();
        }

        public static ClearUnsafeParallelBlockListJob FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<UnsafeParallelBlockList>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.isCreated == false || list.Count() < 1)
            {
                return default;
            }

            return new ClearUnsafeParallelBlockListJob {
                list = list,
            };
        }
    }

    [BurstCompile]
    public partial struct ClearUnsafeIndexedBlockListJob : IJob
    {
        public UnsafeIndexedBlockList list;

        [BurstCompile]
        public void Execute()
        {
            var indexCount = list.indexCount;

            for (var i = 0; i < indexCount; i++)
            {
                list.ClearIndex(i);
            }
        }

        public static ClearUnsafeIndexedBlockListJob FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<UnsafeIndexedBlockList>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.isCreated == false || list.indexCount < 1 || list.Count() < 1)
            {
                return default;
            }

            return new ClearUnsafeIndexedBlockListJob {
                list = list,
            };
        }
    }

    [BurstCompile]
    public partial struct ClearIndexOfUnsafeIndexedBlockListJob : IJob
    {
        public UnsafeIndexedBlockList list;
        public int indexToClear;

        [BurstCompile]
        public void Execute()
        {
            list.ClearIndex(indexToClear);
        }

        public static ClearIndexOfUnsafeIndexedBlockListJob FromCollectionComponent<TCollectionComponent>(
              BlackboardEntity blackboard
            , int indexToClear
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<UnsafeIndexedBlockList>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.isCreated == false
                || list.indexCount < 1
                || (uint)indexToClear >= (uint)list.indexCount
                || list.CountForIndex(indexToClear) < 1
            )
            {
                return default;
            }

            return new ClearIndexOfUnsafeIndexedBlockListJob {
                list = list,
                indexToClear = indexToClear,
            };
        }
    }

    [BurstCompile]
    public partial struct ClearUnsafeParallelBlockListJob<TData> : IJob
        where TData : unmanaged
    {
        public UnsafeParallelBlockList<TData> list;

        [BurstCompile]
        public void Execute()
        {
            list.Clear();
        }

        public static ClearUnsafeParallelBlockListJob<TData> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<UnsafeParallelBlockList<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.isCreated == false || list.Count() < 1)
            {
                return default;
            }

            return new ClearUnsafeParallelBlockListJob<TData> {
                list = list,
            };
        }
    }

    [BurstCompile]
    public partial struct ClearUnsafeIndexedBlockListJob<TData> : IJob
        where TData : unmanaged
    {
        public UnsafeIndexedBlockList<TData> list;

        [BurstCompile]
        public void Execute()
        {
            var indexCount = list.indexCount;

            for (var i = 0; i < indexCount; i++)
            {
                list.ClearIndex(i);
            }
        }

        public static ClearUnsafeIndexedBlockListJob<TData> FromCollectionComponent<TCollectionComponent>(
            BlackboardEntity blackboard
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<UnsafeIndexedBlockList<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.isCreated == false || list.indexCount < 1 || list.Count() < 1)
            {
                return default;
            }

            return new ClearUnsafeIndexedBlockListJob<TData> {
                list = list,
            };
        }
    }

    [BurstCompile]
    public partial struct ClearIndexOfUnsafeIndexedBlockListJob<TData> : IJob
        where TData : unmanaged
    {
        public UnsafeIndexedBlockList<TData> list;
        public int indexToClear;

        [BurstCompile]
        public void Execute()
        {
            list.ClearIndex(indexToClear);
        }

        public static ClearIndexOfUnsafeIndexedBlockListJob<TData> FromCollectionComponent<TCollectionComponent>(
              BlackboardEntity blackboard
            , int indexToClear
        )
            where TCollectionComponent : unmanaged
                , ICollectionComponent
                , Latios.InternalSourceGen.StaticAPI.ICollectionComponentSourceGenerated
                , ITryGet<UnsafeIndexedBlockList<TData>>
        {
            if (blackboard.HasCollectionComponent<TCollectionComponent>() == false)
            {
                return default;
            }

            if (blackboard.GetCollectionComponent<TCollectionComponent>().TryGet(out var list) == false)
            {
                return default;
            }

            if (list.isCreated == false
                || list.indexCount < 1
                || (uint)indexToClear >= (uint)list.indexCount
                || list.CountForIndex(indexToClear) < 1
            )
            {
                return default;
            }

            return new ClearIndexOfUnsafeIndexedBlockListJob<TData> {
                list = list,
                indexToClear = indexToClear,
            };
        }
    }
}

#endif
