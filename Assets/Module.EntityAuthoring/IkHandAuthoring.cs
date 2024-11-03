using System;
using EncosyTower.Modules.Entities;
using Latios;
using Module.EntityComponents;
using Unity.Entities;
using UnityEngine;

namespace Module.EntityAuthoring
{
    internal sealed partial class IkHandAuthoring : MonoBehaviour
    {
        [ReorderableList(Foldable = true)] public HandEntry[] hands;

        [Serializable]
        public struct HandEntry
        {
            public Transform transform;
            public string boneName;
            public byte id;
        }

        private class Baker : Baker<IkHandAuthoring>
        {
            public override void Bake(IkHandAuthoring authoring)
            {
                var hands = authoring.hands.AsSpan();
                var handsLength = hands.Length;

                if (handsLength < 1)
                {
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<IkHandElement>(entity);

                for (var i = 0; i < handsLength; i++)
                {
                    ref readonly var hand = ref hands[i];
                    buffer.Add(new IkHandElement {
                        boneName = hand.boneName,
                        entity = GetEntity(hand.transform, TransformUsageFlags.Renderable),
                        id = hand.id,
                    });
                }
            }
        }
    }
}
