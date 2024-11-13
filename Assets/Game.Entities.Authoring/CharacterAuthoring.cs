using System;
using EncosyTower.Modules.Entities;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Module.EntityComponents;
using Module.GameCommon;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Module.EntityAuthoring
{
    internal sealed partial class CharacterAuthoring : MonoBehaviour
    {
        [ReorderableList(Foldable = true)] public AnimEntry[] anims;

        private sealed class Baker : SmartBaker<CharacterAuthoring, SmartBakeItem> { }

        [Serializable]
        public struct AnimEntry
        {
            public CharAnim anim;
            public AnimationClip clip;
        }

        [TemporaryBakingType]
        private struct SmartBakeItem : ISmartBakeItem<CharacterAuthoring>
        {
            private SmartBlobberHandle<SkeletonClipSetBlob> _animClipBlob;

            public bool Bake(CharacterAuthoring authoring, IBaker baker)
            {
                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<EntityId>(entity);

                var anims = authoring.anims.AsSpan();
                var animsLength = anims.Length;

                if (animsLength < 1)
                {
                    return false;
                }

                var animTypes = new NativeArray<CharAnim>(animsLength, Allocator.Temp);
                var animClips = new NativeArray<SkeletonClipConfig>(animsLength, Allocator.Temp);

                for (var i = 0; i < animsLength; i++)
                {
                    ref var anim = ref anims[i];

                    animTypes[i] = anim.anim;
                    animClips[i] = new SkeletonClipConfig {
                        clip = anim.clip,
                        settings = SkeletonClipCompressionSettings.kDefaultSettings
                    };
                }

                var builder = new BlobBuilder(Allocator.Temp);
                ref var animTypeBlobRoot = ref builder.ConstructRoot<CharAnimTypeBlob>();
                builder.ConstructArray(ref animTypeBlobRoot.types, animTypes);

                baker.AddComponent(entity, new CharAnimTypeBlobRef {
                    value = builder.CreateBlobAssetReference<CharAnimTypeBlob>(Allocator.Persistent)
                });

                baker.AddComponent<AnimClipBlobRef>(entity);
                _animClipBlob = baker.RequestCreateBlobAsset(baker.GetComponentInChildren<Animator>(), animClips);

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
            {
                if (_animClipBlob.IsValid)
                {
                    entityManager.SetComponentData(entity, new AnimClipBlobRef {
                        value = _animClipBlob.Resolve(entityManager)
                    });
                }
            }
        }
    }
}
