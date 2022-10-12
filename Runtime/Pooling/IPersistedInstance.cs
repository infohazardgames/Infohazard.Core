// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

namespace Infohazard.Core {
    /// <summary>
    /// This is a hack so that PoolManager can send messages to PersistedGameObjects.
    /// </summary>
    /// <remarks>
    /// There is likely no need to use this interface yourself.
    /// </remarks>
    public interface IPersistedInstance {
        /// <summary>
        /// Initialize the current object as a new persisted instance with the given ID.
        /// </summary>
        /// <param name="persistedInstanceID">The instance ID to use.</param>
        void SetupDynamicInstance(ulong persistedInstanceID);
        
        /// <summary>
        /// Remove the current object from persistence.
        /// </summary>
        void RegisterDestroyed();
    }
}