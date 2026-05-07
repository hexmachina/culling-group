using UnityEngine;

namespace TW.CullingGroups
{
	public class DynamicCullingGroupObject : CullingGroupObject
	{
		private void FixedUpdate()
		{
			UpdateBounds();
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying && isActiveAndEnabled)
			{
				var pos = _target.GetPoint();
				Gizmos.color = Color.cyan;
				Gizmos.DrawRay(pos - Vector3.up * 0.5f, Vector3.up * 0.5f);
				Gizmos.DrawRay(pos - Vector3.right * 0.5f, Vector3.right);
				Gizmos.DrawRay(pos - Vector3.forward * 0.5f, Vector3.forward);
			}
		}
	}
}
