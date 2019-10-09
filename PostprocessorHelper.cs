using UnityEngine;

namespace ForkLift
{
    /// <summary>
    /// Provides a static conventience method for calling the Processor class.
    /// </summary>
    public static class PostprocessorHelper
    {
        /// <summary>
        /// Import the specified type of asset, running it through the specified importer and validator. 
        /// This version does not have an explicit declaration of field names.
        /// </summary>
        /// <typeparam name="TAsset">the type of asset to import</typeparam>
        /// <typeparam name="TImporter">the importer to use</typeparam>
        /// <typeparam name="TValidator">the validator to use</typeparam>
        /// <param name="asset">the location of the asset</param>
        public static void Import<TAsset, TImporter, TValidator>(string asset) where TAsset : ScriptableObject
                                                                               where TImporter : IImporter<TAsset>
                                                                               where TValidator : IValidator
        {
            new Processor().Import<TAsset, TImporter, TValidator>(asset);
        }
    }
}