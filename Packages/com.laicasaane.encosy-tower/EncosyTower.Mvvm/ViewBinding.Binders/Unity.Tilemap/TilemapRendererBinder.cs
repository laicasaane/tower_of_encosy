#if UNITY_TILEMAP

#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity.Tilemaps
{
    [Serializable, Binder]
    [Label("Tilemap Renderer", "Tilemap")]
    public sealed partial class TilemapRendererBinder : MonoBinder<TilemapRenderer>
    {
    }

    [Serializable, Binder]
    [Label("Chunk Culling Bounds", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingChunkCullingBounds : MonoBindingProperty<TilemapRenderer>
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

    [Serializable, Binder]
    [Label("Chunk Size", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingChunkSize : MonoBindingProperty<TilemapRenderer>
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

    [Serializable, Binder]
    [Label("Detect Chunk Culling Bounds", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingDetectChunkCullingBounds : MonoBindingProperty<TilemapRenderer>
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

    [Serializable, Binder]
    [Label("Mask Interaction", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMaskInteraction : MonoBindingProperty<TilemapRenderer>
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

    [Serializable, Binder]
    [Label("Max Chunk Count", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMaxChunkCount : MonoBindingProperty<TilemapRenderer>
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

    [Serializable, Binder]
    [Label("Max Frame Age", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMaxFrameAge : MonoBindingProperty<TilemapRenderer>
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

    [Serializable, Binder]
    [Label("Mode", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingMode : MonoBindingProperty<TilemapRenderer>
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

    [Serializable, Binder]
    [Label("Sort Order", "Tilemap Renderer")]
    public sealed partial class TilemapRendererBindingSortOrder : MonoBindingProperty<TilemapRenderer>
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
