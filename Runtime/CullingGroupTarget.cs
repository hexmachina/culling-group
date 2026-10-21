using System;
using UnityEngine;

namespace TW.CullingGroups
{
	public struct CullingGroupState
	{
		public Vector3 Position;
		public CullingGroupEvent groupEvent;
		public int colliderIndex;
		public int colliderCount;
	}

	[Serializable]
	public class CullingGroupTarget
	{
		public CullingGroupProfile profile;

		[SerializeField]
		Vector3 m_Center = new(0, 1.0f, 0);
		public Vector3 Center { get => m_Center; set => m_Center = value; }

		[SerializeField]
		private float m_Radius = 1f;

		[SerializeField] bool _isDynamic = false;

		public bool IsDynamic => _isDynamic;

		public float Radius { get => m_Radius; set => m_Radius = value; }

		public Vector3 Offset => transform.TransformPoint(m_Center);

		public Transform transform { get; set; }

		public event Action<CullingGroupEvent> OnStateChanged;

		public bool IsVisible => _current.isVisible;

		public int Distance => _current.currentDistance;

		protected int _currentIndex = -1;

		private CullingGroupEvent _current = new();

		public CullingGroupEvent Current => _current;

		private EntityId[] _colliderIds;

		public int Index => _currentIndex;

		private EntityId _claimId = EntityId.None;

		public EntityId ClaimId
		{
			get
			{
				if (_claimId == EntityId.None)
				{
					_claimId = transform.gameObject.GetEntityId();

				}
				return _claimId;
			}
		}

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

		public bool RefreshIndex(int index)
		{
			if (index == _currentIndex)
			{
				return false;
			}
			_currentIndex = index;
			return true;
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
			return !profile || _currentIndex == -1 ? Vector3.zero : profile.Bounds[_currentIndex].position;
		}

		public virtual void OnStateChange(CullingGroupEvent e)
		{
			_current = e;
			OnStateChanged?.Invoke(e);
		}

		public EntityId[] GetColliderIds()
		{
			if (!transform)
			{
				return null;
			}
			if (_colliderIds == null)
			{
				RefreshColliderCache();
			}

			return _colliderIds;
		}

		private void RefreshColliderCache()
		{
			if (!transform)
			{
				return;
			}
			var colliders = transform.GetComponents<Collider>();
			if (colliders == null || colliders.Length == 0)
			{
				return;
			}
			var length = colliders.Length;
			_colliderIds = new EntityId[length];
			for (int i = 0; i < length; i++)
			{
				_colliderIds[i] = colliders[i].GetEntityId();
			}
		}

		public CullingGroupState GetState()
		{
			return new CullingGroupState()
			{
				Position = Offset,
				groupEvent = Current,
				colliderCount = _colliderIds != null ? _colliderIds.Length : 0,
			};
		}
	}
}
