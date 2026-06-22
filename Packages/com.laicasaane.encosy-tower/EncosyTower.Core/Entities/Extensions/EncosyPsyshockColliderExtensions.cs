#if UNITY_ENTITIES && LATIOS_FRAMEWORK

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.UnityExtensions;
using Unity.Mathematics;
using UnityEngine;

namespace EncosyTower.Entities
{
    using Psyshock = Latios.Psyshock;
    using PsyshockCollider = Latios.Psyshock.Collider;
    using UnityCollider = UnityEngine.Collider;

    public static class EncosyPsyshockColliderExtensions
    {
        public static PsyshockCollider ToPsyshockCollider([NotNull] this UnityCollider collider)
        {
            ThrowIfInvalidCollider(collider.IsValid());

            return collider switch {
                SphereCollider sphere => ToPsyshockCollider(sphere),
                CapsuleCollider capsule => ToPsyshockCollider(capsule),
                BoxCollider box => ToPsyshockCollider(box),
                _ => default,
            };
        }

        public static Psyshock.SphereCollider ToPsyshockCollider([NotNull] this SphereCollider collider)
        {
            ThrowIfInvalidCollider(collider.IsValid());

            return new Psyshock.SphereCollider {
                center = collider.center,
                radius = collider.radius,
                stretchMode = Psyshock.SphereCollider.StretchMode.StretchCenter
            };
        }

        public static Psyshock.CapsuleCollider ToPsyshockCollider([NotNull] this CapsuleCollider collider)
        {
            ThrowIfInvalidCollider(collider.IsValid());

            float3 dir;

            if (collider.direction == 0)
            {
                dir = new float3(1f, 0f, 0f);
            }
            else if (collider.direction == 1)
            {
                dir = new float3(0f, 1, 0f);
            }
            else
            {
                dir = new float3(0f, 0f, 1f);
            }

            return new Psyshock.CapsuleCollider {
                pointB = (float3)collider.center + ((collider.height / 2f - collider.radius) * dir),
                pointA = (float3)collider.center - ((collider.height / 2f - collider.radius) * dir),
                radius = collider.radius,
                stretchMode = Psyshock.CapsuleCollider.StretchMode.StretchPoints
            };
        }

        public static Psyshock.BoxCollider ToPsyshockCollider([NotNull] this BoxCollider collider)
        {
            ThrowIfInvalidCollider(collider.IsValid());

            return new Psyshock.BoxCollider {
                center = collider.center,
                halfSize = collider.size / 2f
            };
        }

        [HideInCallstack, StackTraceHidden, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ThrowIfInvalidCollider([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static System.ArgumentException CreateException()
            {
                return new System.ArgumentException("Collider is null or invalid.", "collider");
            }
        }
    }
}

#endif
