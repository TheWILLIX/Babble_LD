using UnityEngine;

	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {

		#region Fields
		private static T _instance;
		#endregion Fields

		#region Properties
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{	
					_instance = FindObjectOfType<T>();

					if (_instance == null)
					{
						Debug.LogError(typeof(T) + " Trying to access a nulled instance of a singleton.");
					}
				}
				return _instance;
			}
		}
	public static T GetInstanceNoNotify()
    {
		return _instance;
    }
	public static bool HasInstance
    {
        get
        {
			return _instance != null;
        }
    }
		#endregion Properties

		#region Methods
		#region MonoBehaviour
		protected virtual void Awake()
		{
			if (_instance == null)
				_instance = this as T;
		}

		protected virtual void Start()
		{
			DontDestroyOnLoad(this);
		}

		protected virtual void OnEnable() { }
		protected virtual void Update() { }
		protected virtual void LateUpdate() { }
		protected virtual void OnDisable() { }

		protected virtual void OnDestroy()
		{
			_instance = null;
		}
		#endregion MonoBehaviour
		#endregion Methods
	}
