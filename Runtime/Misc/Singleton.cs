using UnityEngine;

namespace Infohazard.Core.Runtime {
    public class SingletonBase : MonoBehaviour {
        public virtual void Initialize() { }
    }
    
    public class Singleton<T> : SingletonBase where T : SingletonBase {
        private static T _instance;

        public static T Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null) {
                        Debug.LogError($"{typeof(T)} instance not found!");
                    }

                    _instance.Initialize();
                }

                return _instance;
            }
        }
    }
}