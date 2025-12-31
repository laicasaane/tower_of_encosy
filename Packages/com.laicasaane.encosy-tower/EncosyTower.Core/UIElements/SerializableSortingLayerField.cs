using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Extensions;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    [UxmlElement(libraryPath = "Encosy Tower")]
    public partial class SerializableSortingLayerField : PopupField<SerializableSortingLayer>, IHasBindingPath
    {
        public static readonly string UssClassName = "serializable-soring-layer-field";

        /// <summary>
        /// Construct a SoringLayerIdField.
        /// </summary>
        public SerializableSortingLayerField()
            : this(null)
        { }

        /// <summary>
        /// Construct a SoringLayerIdField.
        /// </summary>
        public SerializableSortingLayerField(string label)
            : base(label, SortingLayerHelper.GetLayerIds(), 0, IdToString, IdToString)
        {
            AddToClassList(ussClassName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string IdToString(SerializableSortingLayer id)
            => id.Name;

        private static class SortingLayerHelper
        {
            private static readonly List<SerializableSortingLayer> s_layerIds = new();
            private static bool s_init;

            public static List<SerializableSortingLayer> GetLayerIds()
            {
                Init();
                return s_layerIds;
            }

            private static void Init()
            {
                if (s_init)
                {
#if !UNITY_EDITOR
                    return;
#endif
                }

                var layers = SortingLayer.layers.AsSpan();
                var length = layers.Length;
                var layerIds = s_layerIds;

                layerIds.Clear();
                layerIds.IncreaseCapacityTo(length);

                for (var i = 0; i < length; i++)
                {
                    layerIds.Add(new SerializableSortingLayer(layers[i].id));
                }

                s_init = true;
            }
        }
    }
}
