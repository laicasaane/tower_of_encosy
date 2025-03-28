using System;
using System.Reflection;
using System.Threading.Tasks;
using EncosyTower.Databases.Authoring;
using EncosyTower.Logging;
using EncosyTower.Naming;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class DatabaseSettings
        {
            public void Convert(DataSourceFlags sources, UnityEngine.Object owner)
            {
                var result = Validate();

                if (result != ValidationResult.Success)
                {
                    Log(result);
                    return;
                }

                result = TryGetData(out var databaseAssetName, out var sheetContainerType);

                if (result != ValidationResult.Success)
                {
                    Log(result, sheetContainerType);
                    return;
                }

                result = TryCreateSheetContainer(sheetContainerType, out var sheetContainer);

                if (result != ValidationResult.Success)
                {
                    Log(result, sheetContainerType);
                    return;
                }

                if (sources.HasFlag(DataSourceFlags.GoogleSheet) && googleSheetSettings.enabled)
                {
                    googleSheetSettings.Convert(new(databaseAssetName, sheetContainer, owner));
                }

                if (sources.HasFlag(DataSourceFlags.Csv) && csvSettings.enabled)
                {
                    csvSettings.Convert(new(databaseAssetName, sheetContainer, owner));
                }

                if (sources.HasFlag(DataSourceFlags.Excel) && excelSettings.enabled)
                {
                    excelSettings.Convert(new(databaseAssetName, sheetContainer, owner));
                }
            }

            public async Task<DataSourceFlags> ConvertAsync(
                  DataSourceFlags sources
                , UnityEngine.Object owner
                , bool continueOnCapturedContext
            )
            {
                var result = Validate();

                if (result != ValidationResult.Success)
                {
                    Log(result);
                    return DataSourceFlags.None;
                }

                result = TryGetData(out var databaseAssetName, out var sheetContainerType);

                if (result != ValidationResult.Success)
                {
                    Log(result, sheetContainerType);
                    return DataSourceFlags.None;
                }

                result = TryCreateSheetContainer(sheetContainerType, out var sheetContainer);

                if (result != ValidationResult.Success)
                {
                    Log(result, sheetContainerType);
                    return DataSourceFlags.None;
                }

                var resultFlags = DataSourceFlags.None;

                if (sources.HasFlag(DataSourceFlags.GoogleSheet) && googleSheetSettings.enabled)
                {
                    if (await googleSheetSettings.ConvertAsync(
                          new(databaseAssetName, sheetContainer, owner)
                        , continueOnCapturedContext
                    ))
                    {
                        resultFlags |= DataSourceFlags.GoogleSheet;
                    }
                }

                if (sources.HasFlag(DataSourceFlags.Csv) && csvSettings.enabled)
                {
                    if (await csvSettings.ConvertAsync(
                          new(databaseAssetName, sheetContainer, owner)
                        , continueOnCapturedContext
                    ))
                    {
                        resultFlags |= DataSourceFlags.Csv;
                    }
                }

                if (sources.HasFlag(DataSourceFlags.Excel) && excelSettings.enabled)
                {
                    if (await excelSettings.ConvertAsync(
                          new(databaseAssetName, sheetContainer, owner)
                        , continueOnCapturedContext
                    ))
                    {
                        resultFlags |= DataSourceFlags.Excel;
                    }
                }

                return resultFlags;
            }

            private ValidationResult Validate()
            {
                if (string.IsNullOrWhiteSpace(name)
                    || string.Equals(name, "<Undefined>", StringComparison.Ordinal)
                )
                {
                    return ValidationResult.DatabaseNameIsNullOrInvalid;
                }

                if (string.IsNullOrWhiteSpace(authorType))
                {
                    return ValidationResult.AuthorTypeIsNullOrInvalid;
                }

                if (string.IsNullOrWhiteSpace(databaseType))
                {
                    return ValidationResult.DatabaseTypeIsNullOrInvalid;
                }

                return ValidationResult.Success;
            }

            private ValidationResult TryGetData(out string databaseAssetName, out Type sheetContainerType)
            {
                var authorType = Type.GetType(this.authorType, false, false);
                var databaseType = Type.GetType(this.databaseType, false, false);

                databaseAssetName = nameof(DatabaseAsset);
                sheetContainerType = default;

                if (authorType == null)
                {
                    return ValidationResult.AuthorTypeDoesNotExist;
                }

                if (databaseType == null)
                {
                    return ValidationResult.DatabaseTypeDoesNotExist;
                }

                var databaseAttrib = databaseType.GetCustomAttribute<DatabaseAttribute>(true);

                databaseAssetName = string.IsNullOrWhiteSpace(databaseAttrib.AssetName)
                    ? $"{databaseAssetName}_{databaseType.Name}"
                    : databaseAttrib.AssetName;

                databaseAssetName = NamingMap.ConvertName(databaseAssetName, databaseAttrib.NamingStrategy);

                var candidates = authorType.GetMember(
                      "SheetContainer"
                    , MemberTypes.NestedType
                    , BindingFlags.Public
                );

                if (candidates.Length < 1 || candidates[0] is not Type candiate)
                {
                    return ValidationResult.CannotFindSheetContainerTypeInsideType;
                }

                sheetContainerType = candiate;

                return candiate.IsAbstract || typeof(DataSheetContainerBase).IsAssignableFrom(candiate) == false
                    ? ValidationResult.SheetContainerIsInvalid
                    : ValidationResult.Success;
            }

            private ValidationResult TryCreateSheetContainer(Type type, out DataSheetContainerBase instance)
            {
                try
                {
                    instance = Activator.CreateInstance(type) as DataSheetContainerBase;
                    return ValidationResult.Success;
                }
                catch (Exception ex)
                {
                    instance = default;
                    DevLoggerAPI.LogException(ex);
                    return ValidationResult.Success;
                }
            }

            private void Log(ValidationResult result, Type sheetContainerType = null)
            {
                switch (result)
                {
                    case ValidationResult.DatabaseNameIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Database name is null or invalid: '{name}'");
                        break;

                    case ValidationResult.AuthorTypeIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Authoring type is null or invalid: '{authorType}'");
                        break;

                    case ValidationResult.DatabaseTypeIsNullOrInvalid:
                        DevLoggerAPI.LogError($"Database type is null or invalid: '{databaseType}'");
                        break;

                    case ValidationResult.AuthorTypeDoesNotExist:
                        DevLoggerAPI.LogError($"Authoring type does not exist: '{authorType}'");
                        break;

                    case ValidationResult.DatabaseTypeDoesNotExist:
                        DevLoggerAPI.LogError($"Database type does not exist: '{databaseType}'");
                        break;

                    case ValidationResult.CannotFindSheetContainerTypeInsideType:
                        DevLoggerAPI.LogError($"Cannot find the type 'SheetContainer' inside type '{authorType}'");
                        break;

                    case ValidationResult.SheetContainerIsInvalid:
                    {
                        if (sheetContainerType != null)
                        {
                            DevLoggerAPI.LogError(
                                $"'{sheetContainerType.FullName}' must be a non-abstract class derived from " +
                                $"{typeof(DataSheetContainerBase)}."
                            );
                        }

                        break;
                    }

                    case ValidationResult.CannotCreateAnInstanceOfSheetContainerType:
                    {
                        if (sheetContainerType != null)
                        {
                            DevLoggerAPI.LogError(
                                $"Cannot create an instance of '{sheetContainerType.FullName}'."
                            );
                        }
                        break;
                    }
                }
            }

            private enum ValidationResult
            {
                Success,
                DatabaseNameIsNullOrInvalid,
                AuthorTypeIsNullOrInvalid,
                DatabaseTypeIsNullOrInvalid,
                AuthorTypeDoesNotExist,
                DatabaseTypeDoesNotExist,
                CannotFindSheetContainerTypeInsideType,
                SheetContainerIsInvalid,
                CannotCreateAnInstanceOfSheetContainerType,
            }
        }
    }
}
