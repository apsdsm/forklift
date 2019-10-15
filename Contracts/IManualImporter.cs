using UnityEngine;

namespace ForkLift
{
    /// <summary>
    /// Contract defines functionality for any class that acts as importer.
    /// </summary>
    public interface IManualImporter<out T> where T : ScriptableObject
    {
        /// <summary>
        /// Import data.
        /// </summary>
        /// <param name="assetPath">filepath of data to import</param>
        /// <param name="assetData">source of data to import</param>
        /// <returns>Instance of generated ScriptableObject</returns>
        void Import(string sourceAssetPath, ImportData assetData);
    }
}