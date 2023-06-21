// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

namespace Infohazard.Core {
    public interface IPoolHandler {
        /// <summary>
        /// Current number of users of the <see cref="IPoolHandler"/>.
        /// </summary>
        public int RetainCount { get; }

        /// <summary>
        /// Spawn an instance from the pool.
        /// </summary>
        /// <returns>The spawned instance.</returns>
        public Spawnable Spawn();

        /// <summary>
        /// Despawn an instance, releasing it back to the pool.
        /// </summary>
        /// <param name="instance">The instance to despawn.</param>
        public void Despawn(Spawnable instance);

        /// <summary>
        /// Add a user of the <see cref="IPoolHandler"/>.
        /// </summary>
        public void Retain();

        /// <summary>
        /// Remove a user of the <see cref="IPoolHandler"/>.
        /// </summary>
        public void Release();
    }
}