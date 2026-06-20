#if UNITY_ADDRESSABLES

using System;
using EncosyTower.Common;

namespace UnityEngine.ResourceManagement.AsyncOperations
{
    public static class EncosyAsyncOperationHandleExtensions
    {
        /// <summary>
        /// Attempts to release the AsyncOperationHandle safely,
        /// ignoring any exceptions that may occur during the release process.
        /// </summary>
        public static Success<Exception> TryRelease<T>(this AsyncOperationHandle<T> self)
        {
            try
            {
                self.Release();
                return Success.Yes;
            }
            catch (Exception ex)
            {
                // Ignore exceptions during release
                return Success.No(ex);
            }
        }

        /// <summary>
        /// Attempts to release the AsyncOperationHandle safely,
        /// ignoring any exceptions that may occur during the release process.
        /// </summary>
        public static Success<Exception> TryRelease<T>(this AsyncOperationHandle self)
        {
            try
            {
                self.Release();
                return Success.Yes;
            }
            catch (Exception ex)
            {
                // Ignore exceptions during release
                return Success.No(ex);
            }
        }
    }
}

#endif
