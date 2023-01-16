// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

namespace Infohazard.Core {
    public interface IPoolHandler {
        public int RetainCount { get; }
        
        public Spawnable Spawn();

        public void Despawn(Spawnable instance);

        public void Retain();

        public void Release();
    }
}