using UnityEngine;
using UnityEngine.Events;

namespace TW.CullingGroups
{
	public class CullingGroupListener : MonoBehaviour
	{
		public UnityEvent<bool> onVisibilityChanged = new();
		public UnityEvent<bool> onRangeChanged = new();

		public void OnStateChange(CullingGroupEvent e)
		{

			onVisibilityChanged.Invoke(e.isVisible);
			onRangeChanged.Invoke(e.currentDistance > 2);
		}
	}

}

