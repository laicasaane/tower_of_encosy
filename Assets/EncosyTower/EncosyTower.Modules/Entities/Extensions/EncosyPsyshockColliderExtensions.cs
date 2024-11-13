#if UNITY_ENTITIES && LATIOS_FRAMEWORK

using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;

namespace EncosyTower.Modules.Entities
{
    using Psyshock = Latios.Psyshock;
    using PsyshockCollider = Latios.Psyshock.Collider;
    using UnityCollider = UnityEngine.Collider;

    public static class EncosyPsyshockColliderExtensions
    {
        public static PsyshockCollider ToPsyshockCollider([NotNull] this UnityCollider collider)
        {
            if (collider == false)
            {
                return default;
            }

            return collider switch {
                UnityEngine.SphereCollider sphere => ToPsyshockCollider(sphere),
                UnityEngine.CapsuleCollider capsule => ToPsyshockCollider(capsule),
                UnityEngine.BoxCollider box => ToPsyshockCollider(box),
                _ => default,
            };
        }

        public static Psyshock.SphereCollider ToPsyshockCollider([NotNull] this UnityEngine.SphereCollider collider)
        {
            if (collider == false)
            {
                return default;
            }

            return new Psyshock.SphereCollider {
                center = collider.center,
                radius = collider.radius,
                stretchMode = Psyshock.SphereCollider.StretchMode.StretchCenter
            };
        }

        public static Psyshock.CapsuleCollider ToPsyshockCollider([NotNull] this UnityEngine.CapsuleCollider collider)
        {
            if (collider == false)
            {
                return default;
            }

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

        public static Psyshock.BoxCollider ToPsyshockCollider([NotNull] this UnityEngine.BoxCollider collider)
        {
            if (collider == false)
            {
                return default;
            }

            return new Psyshock.BoxCollider {
                center = collider.center,
                halfSize = collider.size / 2f
            };
        }
    }
}

#endif
