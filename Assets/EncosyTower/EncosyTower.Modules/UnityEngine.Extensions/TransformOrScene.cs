using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules
{
    public readonly struct TransformOrScene
    {
        public const byte TRANSFORM_FLAG = 0b_0000_1111;
        public const byte SCENE_FLAG = 0b_1111_0000;

        private readonly byte _flag;

        public readonly Transform Transform;
        public readonly Scene Scene;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransformOrScene(Transform transform) : this()
        {
            _flag = TRANSFORM_FLAG;
            Transform = transform;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransformOrScene(Scene scene) : this()
        {
            _flag = SCENE_FLAG;
            Scene = scene;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (IsTransform && Transform) || (IsScene && Scene.IsValid());
        }

        public bool IsTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flag == TRANSFORM_FLAG;
        }

        public bool IsScene
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flag == SCENE_FLAG;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TransformOrScene(Transform transform)
            => new(transform);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TransformOrScene(Scene scene)
            => new(scene);
    }
}
