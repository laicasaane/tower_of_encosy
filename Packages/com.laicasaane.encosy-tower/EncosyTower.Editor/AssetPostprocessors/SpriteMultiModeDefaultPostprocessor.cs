#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.AssetPostprocessors
{
    /// <summary>
    /// Since Unity 6, the Sprite Import Mode is default to <see cref="SpriteImportMode.Multiple"/>,
    /// as soon as the Texture Type is set to <see cref="TextureImporterType.Sprite"/>.
    /// However the Texture Importer does not create a default sprite but leaves it to the users.
    /// <br/>
    /// This post-processor will create a single sprite for each Texture whenever these conditions are satisfied:
    /// <list type="bullet">
    /// <item><c>Texture type</c> is <see cref="TextureImporterType.Sprite"/></item>
    /// <item><c>Sprite Import Mode</c> is <see cref="SpriteImportMode.Multiple"/></item>
    /// <item><c>Sprites</c> array is empty.</item>
    /// </list>
    /// </summary>
    /// <example>
    /// This functionality is invoked as soon as the Texture Type is set to <see cref="TextureImporterType.Sprite"/>.
    /// </example>
    public class SpriteMultiModeDefaultPostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (assetImporter is not TextureImporter textureImporter
                || textureImporter.importSettingsMissing is true
                || textureImporter.textureType is not TextureImporterType.Sprite
                || textureImporter.spriteImportMode is not SpriteImportMode.Multiple
            )
            {
                return;
            }

#if UNITY_2D_SPRITE
            ProcessUsingNewerMethod(textureImporter);
#else
            ProcessUsingObsoleteMethod(textureImporter);
#endif
        }

#if UNITY_2D_SPRITE
        /// <remarks>
        /// This method must have a reference to the <c>Unity.2D.Sprite.Editor</c> assembly (asmdef).
        /// </remarks>
        private void ProcessUsingNewerMethod(TextureImporter importer)
        {
            var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
            factory.Init();

            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(assetImporter);

            if (dataProvider == null)
            {
                return;
            }

            dataProvider.InitSpriteEditorDataProvider();

            var rects = dataProvider.GetSpriteRects();

            if (rects.Length > 0)
            {
                return;
            }

            importer.GetSourceTextureWidthAndHeight(out var width, out var height);

            dataProvider.SetSpriteRects(new SpriteRect[] {
                new() {
                    name = System.IO.Path.GetFileNameWithoutExtension(importer.assetPath),
                    alignment = SpriteAlignment.Center,
                    rect = new(Vector2.zero, new(width, height)),
                }
            });

            dataProvider.Apply();
        }
#endif

        /// <remarks>
        /// This method uses the obsolete property <see cref="TextureImporter.spritesheet"/>.
        /// </remarks>
        [System.Obsolete, System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        private void ProcessUsingObsoleteMethod(TextureImporter importer)
        {
            if (importer.spritesheet.Length > 0)
            {
                return;
            }

            importer.GetSourceTextureWidthAndHeight(out var width, out var height);

            importer.spritesheet = new SpriteMetaData[] {
                new() {
                    name = System.IO.Path.GetFileNameWithoutExtension(importer.assetPath),
                    alignment = (int)SpriteAlignment.Center,
                    rect = new(Vector2.zero, new(width, height)),
                }
            };
        }
    }
}

#endif
