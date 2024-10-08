#if ENABLE_TILEMAP

using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Tilemaps
{
    [Serializable]
    [Label("Tilemap Renderer", "Tilemap")]
    public sealed partial class TilemapRendererBinder : MonoBinder<TilemapRenderer>
    {
    }

    [Serializable]
    [Label("Chunk Culling Bounds", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingChunkCullingBounds : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetChunkCullingBounds(in Vector3 value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].chunkCullingBounds = value;
            }
        }
    }

    [Serializable]
    [Label("Chunk Size", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingChunkSize : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetChunkSize(in Vector3Int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].chunkSize = value;
            }
        }
    }

    [Serializable]
    [Label("Detect Chunk Culling Bounds", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingDetectChunkCullingBounds : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetDetectChunkCullingBounds(TilemapRenderer.DetectChunkCullingBounds value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].detectChunkCullingBounds = value;
            }
        }
    }

    [Serializable]
    [Label("Mask Interaction", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMaskInteraction : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaskInteraction(SpriteMaskInteraction value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maskInteraction = value;
            }
        }
    }

    [Serializable]
    [Label("Max Chunk Count", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMaxChunkCount : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxChunkCount(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxChunkCount = value;
            }
        }
    }

    [Serializable]
    [Label("Max Frame Age", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMaxFrameAge : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxFrameAge(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxFrameAge = value;
            }
        }
    }

    [Serializable]
    [Label("Mode", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMode : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMode(TilemapRenderer.Mode value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].mode = value;
            }
        }
    }

    [Serializable]
    [Label("Sort Order", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingSortOrder : MonoBindingProperty<TilemapRenderer>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSortOrder(TilemapRenderer.SortOrder value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sortOrder = value;
            }
        }
    }
}

#endif
