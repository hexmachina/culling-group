using System;
using UnityEngine;

namespace TW.CullingGroups
{
	[Serializable]
	public class CullingGroupTarget
	{
		public CullingGroupProfile profile;

		[SerializeField]
		Vector3 m_Center = new(0, 1.0f, 0);
		public Vector3 Center { get => m_Center; set => m_Center = value; }

		[SerializeField]
		private float m_Radius = 1f;

		public float Radius { get => m_Radius; set => m_Radius = value; }

		public Vector3 Offset => transform.TransformPoint(m_Center);

		public Transform transform { get; set; }

		public event Action<CullingGroupEvent> onStateChanged;

		public bool IsVisible => _current.isVisible;

		public int Distance => _current.currentDistance;

		protected int _currentIndex = -1;

		private CullingGroupEvent _current = new();

		public void Register()
		{
			if (!profile)
				return;

			_currentIndex = profile.RegisterObject(this);
		}

		public void Unregister()
		{
			if (!profile)
				return;
			profile.UnregisterObject(this);
			_currentIndex = -1;
		}

		public void RefeshIndex(int index)
		{
			_currentIndex = index;
		}

		public virtual void UpdateBounds()
		{
			if (!profile && _currentIndex != -1)
				return;
			if (!transform)
			{
				return;
			}
			profile.UpdateBounds(_currentIndex, Offset, Radius);
		}

		public Vector3 GetPoint()
		{
			if (!profile || _currentIndex == -1)
				return Vector3.zero;

			return profile.Bounds[_currentIndex].position;
		}

		public virtual void OnStateChange(CullingGroupEvent e)
		{
			_current = e;
			onStateChanged?.Invoke(e);
		}
	}
}
