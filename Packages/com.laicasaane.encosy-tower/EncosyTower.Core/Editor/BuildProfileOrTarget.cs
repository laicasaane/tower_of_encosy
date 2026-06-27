#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Core;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    public readonly struct BuildProfileOrTarget
    {
        private readonly BuildProfile _profile;
        private readonly NamedBuildTarget _target;

        private BuildProfileOrTarget(BuildProfile profile, NamedBuildTarget target)
        {
            _profile = profile;
            _target = target;
        }

        public bool IsValid => _profile.IsValid() || _target.TargetName.IsNotEmpty();

        public bool IsProfile => _profile.IsValid();

        public bool IsTarget => _profile.IsValid() == false;

        public string Name
        {
            get
            {
                ThrowIfInvalid(IsValid);

                if (IsProfile)
                {
                    return _profile.name;
                }
                else
                {
                    return _target.TargetName;
                }
            }
        }

        public BuildProfile Profile
        {
            get
            {
                if (IsProfile == false)
                {
                    throw new InvalidOperationException("BuildProfileOrTarget does not contain a BuildProfile.");
                }

                return _profile;
            }
        }

        public NamedBuildTarget Target
        {
            get
            {
                if (IsTarget == false)
                {
                    throw new InvalidOperationException("BuildProfileOrTarget does not contain a NamedBuildTarget.");
                }

                return _target;
            }
        }

        [ApiForEditor]
        public static implicit operator BuildProfileOrTarget(BuildProfile profile)
            => new(profile, default);

        [ApiForEditor]
        public static implicit operator BuildProfileOrTarget(NamedBuildTarget target)
            => new(default, target);

        public EditorBuildSettingsScene[] GetScenesForBuild()
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                return _profile.GetScenesForBuild();
            }
            else
            {
                return EditorBuildSettings.scenes;
            }
        }

        public HashSet<string> GetScriptingDefineSymbols()
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                return BuildAPI.GetScriptingDefineSymbols(_profile);
            }
            else
            {
                return BuildAPI.GetScriptingDefineSymbols(_target);
            }
        }

        public void SetScriptingDefineSymbols(params string[] symbols)
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                BuildAPI.SetScriptingDefineSymbols(_profile, symbols);
            }
            else
            {
                BuildAPI.SetScriptingDefineSymbols(_target, symbols);
            }
        }

        public void SetScriptingDefineSymbols(IEnumerable<string> symbols)
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                BuildAPI.SetScriptingDefineSymbols(_profile, symbols);
            }
            else
            {
                BuildAPI.SetScriptingDefineSymbols(_target, symbols);
            }
        }

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("A valid BuildProfileOrTarget must be acquired from BuildAPI.ActiveProfileOrTarget.");
        }
    }
}

#endif
