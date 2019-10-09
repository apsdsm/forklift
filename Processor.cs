using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ForkLift
{
    public class Processor
    {
        string projectRoot;

        public Processor()
        {
            projectRoot = "Assets";
        }

        /// <summary>
        /// Set the Project root for importing. This allows ForkLift to better grok where you want to
        /// save your assets, and is most useful when you are storing your importables somewhere other
        /// than the root Assets directory.
        /// 
        /// For example, say you were wavng your Excel files in a folder called `Assets/Foo/Bar/XLSX`. You
        /// could set the project root to `Assets/Foo/Bar` and ForkLift would know to save the imported
        /// assets in `Assets/Foo/Bar/Resources` instead of `Assets/Resources`.
        /// </summary>
        /// <param name="root">the root folder of your project</param>
        /// <returns>the processor object for chaining</returns>
        public Processor SetProjectRoot(string root)
        {
            projectRoot = root;
            return this;
        }

        /// <summary>
        /// Import the specified type of asset, running it through the specified importer and validator. 
        /// This version does not have an explicit declaration of field names.
        /// </summary>
        /// <typeparam name="TAsset">the type of asset to import</typeparam>
        /// <typeparam name="TImporter">the importer to use</typeparam>
        /// <typeparam name="TValidator">the validator to use</typeparam>
        /// <param name="asset">the location of the asset</param>
        public void Import<TAsset, TImporter, TValidator>(string asset) where TAsset : ScriptableObject
                                                                        where TImporter : IImporter<TAsset>
                                                                        where TValidator : IValidator
        {
            if (!IsImportableExcelFile(asset))
            {
                Debug.Log("PROCESSING ERROR: File is not processable format.");
                return;
            }

            if (!asset.StartsWith(projectRoot))
            {
                Debug.Log("PROCESSING ERROR: Cannot find project root '" + projectRoot + "' in asset path '" + asset + "'.");
                return;
            }

            var data = new ImportData();
            var reader = new ForkLift.Readers.ExcelReader();
            var validationRunner = new ValidationRunner();
            var validator = Activator.CreateInstance<TValidator>();

            reader.ReadAsset(asset, ref data);

            if (!validationRunner.IsValid(data, validator))
            {
                Debug.Log("ERROR: File is not valid");
                // @todo display error messages
                return;
            }

            var scriptableObject = LoadOrCreateAsset<TAsset>(asset);
            var userImporter = Activator.CreateInstance<TImporter>();
            userImporter.Import((TAsset)scriptableObject, data);
            EditorUtility.SetDirty(scriptableObject);
        }

        /// <summary>
        /// Will return true if the filename matches an importable Excel file. Excel files are importable if:
        /// - they have an .xlsx extension
        /// - they don't contain ~$ in the filename (indicates a temp file)
        /// </summary>
        /// <param name="filename">file name to check</param>
        /// <returns>true if file is importable excel file</returns>
        private bool IsImportableExcelFile(string filename)
        {
            return filename.EndsWith(".xlsx") && !filename.Contains("~$");
        }

        /// <summary>
        /// Will either load an existing asset of the specified type, or create a new one.
        /// </summary>
        /// <typeparam name="T">Type of asset to create</typeparam>
        /// <param name="assetPath">where to load from or create</param>
        /// <returns>the scriptiable object that was loaded or created.</returns>
        private ScriptableObject LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            string assetFileName = Path.GetFileName(assetPath);
            string assetDirName = Path.GetDirectoryName(assetPath);
            string assetDirNameInternal = assetDirName.TrimStart(projectRoot.ToCharArray());

            // after trimming the project root from the input asset file directory, if the save dir is not the only 
            // directory left in the string (e.g, "Foo/Bar" as opposed to "Foo" or ""), we remove the first element 
            // from the string and return the rest. Otherwise if there is only a single path component left in the 
            // string, we return an empty string.
            //
            // e.g., if the asset is saved at "Asset/Excel/Foo/Bar" the internal path (after `Excel`) is "Foo/Bar".
            if (Regex.IsMatch(assetDirNameInternal, @".\/"))
            {
                assetDirNameInternal = Regex.Replace(assetDirNameInternal, @"^\/?.*\/(.*)", @"$1");
            }
            else
            {
                assetDirNameInternal = "";
            }

            string saveDirName = projectRoot + "/Resources/" + assetDirNameInternal;
            string saveFileName = Path.Combine(saveDirName, Path.GetFileNameWithoutExtension(assetFileName)) + ".asset";

            Directory.CreateDirectory(saveDirName);
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath(saveFileName, typeof(T)) as ScriptableObject;

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, saveFileName);
            }

            return asset;
        }
    }
}