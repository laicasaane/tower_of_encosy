using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EncosyTower.Annotations;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.ComponentModel.SourceGen;
using EncosyTower.Mvvm.Input.SourceGen;
using EncosyTower.Naming;
using EncosyTower.StringIds;
using EncosyTower.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.VisualDebugging.Commands
{
    public static partial class VisualCommanderAPI
    {
        private static ArrayMap<StringId, FasterList<VisualCommandData>> s_directoryToCommands;
        private static FasterList<VisualDirectoryData> s_directories;

        public static VisualCommanderView CreateView([NotNull] VisualElement root, float directoryListWidth)
        {
            AggregateCommands();

            var view = new VisualCommanderView(directoryListWidth);
            var controller = new VisualCommanderViewController(view);
            controller.Initialize(s_directoryToCommands, s_directories.AsMemory());

            root.Add(view);
            return view;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitWhenDomainReloadDisabled()
        {
            s_directoryToCommands = null;
            s_directories = null;
        }
#endif

        private static void AggregateCommands()
        {
            if (s_directoryToCommands is not null)
            {
                return;
            }

            var commandTypes = RuntimeTypeCache.GetTypesDerivedFrom<IVisualCommand>();
            var commands = Filter(commandTypes.Span);
            Map(commands.AsReadOnlySpan());

            return;

            static FasterList<VisualCommandData> Filter(ReadOnlySpan<Type> commandTypes)
            {
                var typeOfVoid = typeof(void);
                var typeOfOption = typeof(VisualOption);
                var typeOfEnum = typeof(Enum);
                var typeOfStringList = typeof(IReadOnlyList<string>);
                var typeOfObservableObject = typeof(IObservableObject);
                var orderedProperties = new FasterList<VisualPropertyData>();
                var unorderedProperties = new FasterList<VisualPropertyData>();
                var orderedCommands = new FasterList<VisualCommandData>(commandTypes.Length);
                var unorderedCommands = new FasterList<VisualCommandData>(commandTypes.Length);
                var commandMap = new Dictionary<string, Type>();
                var propertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var optionsFlags = propertyFlags | BindingFlags.Static;
                var commandNameFormat = "Set{0}Command";
                var setOptionForCommandNameFormat = "SetOptionFor{0}Command";

                foreach (var commandType in commandTypes)
                {
                    if (commandType.IsValueType
                        || commandType.IsAbstract
                        || commandType.IsSealed == false
                        || typeOfObservableObject.IsAssignableFrom(commandType) == false
                        || commandType.GetConstructor(Type.EmptyTypes) == null
                    )
                    {
                        LogErrorNotApplicableCommandType(commandType);
                        continue;
                    }

                    orderedProperties.Clear();
                    unorderedProperties.Clear();

                    var commandInfoCandidates = commandType.GetCustomAttributes<RelayCommandInfoAttribute>();

                    foreach (var commandInfo in commandInfoCandidates)
                    {
                        commandMap[commandInfo.CommandName] = commandInfo.ParameterType;
                    }

                    var propertyInfoCandidates = commandType.GetCustomAttributes<NotifyPropertyChangedInfoAttribute>();

                    foreach (var propertyInfo in propertyInfoCandidates)
                    {
                        var propertyName = propertyInfo.PropertyName;
                        var propType = propertyInfo.PropertyType;
                        var visualPropType = DeterminePropertyType(propType);

                        if (visualPropType == VisualPropertyType.Undefined)
                        {
                            LogWarningNotApplicablePropertyType(commandType, propertyName);
                            continue;
                        }

                        var property = commandType.GetProperty(propertyName, propertyFlags);

                        if (property == null
                            || property.GetCustomAttribute<VisualIgnoredAttribute>() != null
                        )
                        {
                            continue;
                        }

                        {
                            var commandName = string.Format(commandNameFormat, propertyName);
                            var argType = visualPropType == VisualPropertyType.Enum
                                ? typeOfEnum
                                : propType;

                            if (commandMap.TryGetValue(commandName, out var commandArgType) == false
                                || argType != commandArgType
                            )
                            {
                                LogWarningMissingRelayCommand(commandType, propertyName, argType);
                                continue;
                            }
                        }

                        VisualOptionsData optionsData = null;

                        if (property.GetCustomAttribute<VisualOptionsAttribute>() is { } optionsAttrib
                            && commandType.GetMethod(optionsAttrib.OptionsGetter, propertyFlags) is { } optionsGetter
                            && typeOfStringList.IsAssignableFrom(optionsGetter.ReturnType)
                            && optionsGetter.GetParameters().Length == 0
                        )
                        {
                            var commandName = string.Format(setOptionForCommandNameFormat, propertyName);

                            if (commandMap.TryGetValue(commandName, out var commandArgType) == false
                                || commandArgType != typeOfOption
                            )
                            {
                                LogWarningMissingSetOptionForCommand(commandType, propertyName, typeOfOption);
                            }
                            else
                            {
                                optionsData = new(optionsGetter, commandName, optionsAttrib.IsDataStatic);
                            }
                        }

                        var propertyOrder = property.GetCustomAttribute<VisualOrderAttribute>()?.Order;
                        var propertyLabelAttrib = property.GetCustomAttribute<LabelAttribute>();
                        var propertyList = propertyOrder.HasValue ? orderedProperties : unorderedProperties;
                        var propertyLabel = (propertyLabelAttrib?.Label).NotEmptyOr(propertyName);
                        var name = propertyName.ToKebabCase();

                        Enum defaultEnumValue = visualPropType == VisualPropertyType.Enum
                            ? Activator.CreateInstance(propType) as Enum
                            : default;

                        propertyList.Add(new VisualPropertyData(
                              name
                            , propertyName
                            , visualPropType
                            , propertyOrder ?? 0
                            , propertyLabel
                            , defaultEnumValue
                            , optionsData
                        ));
                    }

                    if (orderedProperties.Count > 0)
                    {
                        orderedProperties.Sort(static (x, y) => x.Order.CompareTo(y.Order));
                    }

                    if (unorderedProperties.Count > 0)
                    {
                        orderedProperties.AddRange(unorderedProperties.AsSpan());
                    }

                    var command = Activator.CreateInstance(commandType) as IVisualCommand;
                    var commandOrder = commandType.GetCustomAttribute<VisualOrderAttribute>()?.Order;
                    var commandLabelAttrib = commandType.GetCustomAttribute<LabelAttribute>();
                    var commandList = commandOrder.HasValue ? orderedCommands : unorderedCommands;
                    var commandLabel = (commandLabelAttrib?.Label).NotEmptyOr(commandType.Name);
                    var propertyArray = orderedProperties.Count > 0
                        ? orderedProperties.ToArray()
                        : Array.Empty<VisualPropertyData>();

                    commandList.Add(new VisualCommandData(
                          commandType.Name.ToKebabCase()
                        , command
                        , propertyArray
                        , commandOrder ?? 0
                        , commandLabel
                        , CreateDirectoryData((commandLabelAttrib?.Directory).NotEmptyOr(commandLabel))
                    ));
                }

                if (orderedCommands.Count > 0)
                {
                    orderedCommands.Sort(static (x, y) => x.Order.CompareTo(y.Order));
                }

                if (unorderedCommands.Count > 0)
                {
                    orderedCommands.AddRange(unorderedCommands.AsSpan());
                }

                return orderedCommands;
            }

            static VisualDirectoryData CreateDirectoryData(string label)
            {
                var id = StringToId.MakeFromManaged(label);
                var name = label.Replace(' ', '_').Replace('/', '-').ToKebabCase();
                return new VisualDirectoryData(name, label, id);
            }

            static void Map(ReadOnlySpan<VisualCommandData> commands)
            {
                var length = commands.Length;
                var map = s_directoryToCommands = new ArrayMap<StringId, FasterList<VisualCommandData>>(length);
                var directories = s_directories = new FasterList<VisualDirectoryData>(length);

                for (var i = 0; i < length; i++)
                {
                    var command = commands[i];
                    var directory = command.Directory;

                    if (map.TryGetValue(directory.Id, out var indices) == false)
                    {
                        map[directory.Id] = indices = new FasterList<VisualCommandData>();
                        directories.Add(directory);
                    }

                    indices.Add(command);
                }
            }
        }

        private static VisualPropertyType DeterminePropertyType(Type type)
        {
            if (type.IsEnum)
                return VisualPropertyType.Enum;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return VisualPropertyType.Bool;
                case TypeCode.Char: return VisualPropertyType.String;
                case TypeCode.DateTime: return VisualPropertyType.DateTime;
                case TypeCode.Double: return VisualPropertyType.Double;
                case TypeCode.Int32: return VisualPropertyType.Integer;
                case TypeCode.Int64: return VisualPropertyType.Long;
                case TypeCode.Single: return VisualPropertyType.Float;
                case TypeCode.String: return VisualPropertyType.String;
                case TypeCode.UInt32: return VisualPropertyType.UnsignedInteger;
                case TypeCode.UInt64: return VisualPropertyType.UnsignedLong;
            }

            if (type == typeof(Bounds))
                return VisualPropertyType.Bounds;

            if (type == typeof(BoundsInt))
                return VisualPropertyType.BoundsInt;

            if (type == typeof(Rect))
                return VisualPropertyType.Rect;

            if (type == typeof(RectInt))
                return VisualPropertyType.RectInt;

            if (type == typeof(Vector2))
                return VisualPropertyType.Vector2;

            if (type == typeof(Vector2Int))
                return VisualPropertyType.Vector2Int;

            if (type == typeof(Vector3))
                return VisualPropertyType.Vector3;

            if (type == typeof(Vector3Int))
                return VisualPropertyType.Vector3Int;

            if (type == typeof(Vector4))
                return VisualPropertyType.Vector4;

            return VisualPropertyType.Undefined;
        }

        private static void LogErrorNotApplicableCommandType(Type type)
        {
            StaticDevLogger.LogError(
                $"Type '{type}' is not applicable as an {nameof(IVisualCommand)}. " +
                $"It must be a sealed class that also implements {nameof(IObservableObject)} interface " +
                "and has a default constructor."
            );
        }

        private static void LogWarningNotApplicablePropertyType(Type type, string propertyName)
        {
            StaticDevLogger.LogWarning(
                $"Property '{propertyName}' of type '{type}' must only return one of the following types: " +
                "bool, double, float, int, long, string, uint, ulong, " +
                "Bounds, BoundsInt, DateTime, Rect, RectInt, Vector2, Vector2Int, Vector3, Vector3Int, Vector4, " +
                "or an enum."
            );
        }

        private static void LogWarningMissingRelayCommand(Type type, string propertyName, Type argType)
        {
            StaticDevLogger.LogWarning(
                $"Property '{propertyName}' of type '{type}' must have a corresponding method " +
                $"named 'Set{propertyName}'. The method must be annotated with [RelayCommand], " +
                $"and has a single parameter of type '{argType}'."
            );
        }

        private static void LogWarningMissingSetOptionForCommand(Type type, string propertyName, Type argType)
        {
            StaticDevLogger.LogWarning(
                $"Property '{propertyName}' of type '{type}' must have a corresponding method " +
                $"named 'SetOptionFor{propertyName}'. The method must be annotated with [RelayCommand], " +
                $"and has a single parameter of type '{argType}'."
            );
        }
    }
}
