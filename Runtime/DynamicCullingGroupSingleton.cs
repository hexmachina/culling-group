using UnityEngine;

namespace TW.CullingGroups
{
	public class DynamicCullingGroupSingleton : MonoBehaviour
	{
		protected static DynamicCullingGroupSingleton _instance;

		public static DynamicCullingGroupSingleton Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindFirstObjectByType<DynamicCullingGroupSingleton>();
					if (_instance == null)
					{
						var go = new GameObject(typeof(DynamicCullingGroupSingleton).Name);
						_instance = go.AddComponent<DynamicCullingGroupSingleton>();
					}
				}
				return _instance;
			}
		}

		public static bool IsInstantiated => _instance != null;

		public CullingGroupProfile Profile { get; set; }


		public void FixedUpdate()
		{
			if (!Profile)
			{
				return;
			}

			foreach (var item in Profile.Objects)
			{
				if (item.IsDynamic)
				{
					item.UpdateBounds();
				}
			}
		}
	}
}
