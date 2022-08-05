namespace Infohazard.Core {
    /// <summary>
    /// This is a hack so that PoolManager can send messages to PersistedGameObjects.
    /// </summary>
    public interface IPersistedInstance {
        void SetupDynamicInstance(ulong persistedInstanceID);
        void RegisterDestroyed();
    }
}