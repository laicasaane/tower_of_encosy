using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity.Physics3D
{
    [Serializable]
    [Label("Box Collider", "Physics 3D")]
    public sealed partial class MeshColliderBinder : MonoBinder<MeshCollider>
    {
    }

    [Serializable]
    [Label("Convex", "Mesh Collider")]
    public sealed partial class MeshColliderBindingConvex : MonoBindingProperty<MeshCollider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetConvex(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].convex = value;
            }
        }
    }

    [Serializable]
    [Label("Cooking Options", "Mesh Collider")]
    public sealed partial class MeshColliderBindingCookingOptions : MonoBindingProperty<MeshCollider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCookingOptions(MeshColliderCookingOptions value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].cookingOptions = value;
            }
        }
    }

    [Serializable]
    [Label("Shared Mesh", "Mesh Collider")]
    public sealed partial class MeshColliderBindingSharedMesh : MonoBindingProperty<MeshCollider>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSharedMesh(Mesh value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sharedMesh = value;
            }
        }
    }
}
