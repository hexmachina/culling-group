using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW.CullingGroups
{

	[CreateAssetMenu(menuName = "Data/Culling Group/Profile")]
	public class CullingGroupProfile : ScriptableObject
	{

		public float distance = 40;
		[SerializeField] protected float m_Cascade2Split = 0.25f;
		public float Cascade2Split => m_Cascade2Split;

		public IReadOnlyList<CullingGroupTarget> Objects => _objects;

		[NonSerialized] private readonly List<CullingGroupTarget> _objects = new();

		public ObjectsCallbackDelegate onObjectsChanges;
		public ObjectCallbackDelegate onObjectChanges;
		public ObjectCallbackDelegate onRemoved;
		public event Action<CullingGroupTarget> OnAdded;
		public event Action<CullingGroupEvent> OnCullingGroupEvent;

		public delegate void ObjectsCallbackDelegate(List<CullingGroupTarget> objects);
		public delegate void ObjectCallbackDelegate(int index);

		[NonSerialized] protected Transform _referencePoint;

		public Transform ReferencePoint
		{
			get => _referencePoint;

			set
			{
				if (_referencePoint != value)
				{

					_referencePoint = value;
					_cullingGroup?.SetDistanceReferencePoint(_referencePoint);

				}

			}
		}

		[NonSerialized] private CullingGroup _cullingGroup;
		protected const int objectCount = 5000;

		[NonSerialized] private bool _isInitialized = false;

		public BoundingSphere[] Bounds => _bounds;

		[NonSerialized] protected BoundingSphere[] _bounds;

		[NonSerialized] private int _length = 0;

		public void Initialize()
		{
			_isInitialized = true;
			Application.quitting += OnQuitting;
			_cullingGroup = new CullingGroup
			{
				onStateChanged = StateChanged
			};
			var cam = Camera.main;
			if (!cam)
			{
				cam = Camera.current;
			}
			_cullingGroup.targetCamera = cam;
			var point = _referencePoint;
			if (!point)
			{
				point = cam.transform;
			}
			_cullingGroup.SetDistanceReferencePoint(point);

			var cascade0 = distance * Cascade2Split;

			_cullingGroup.SetBoundingDistances(new float[] { cascade0, distance, float.PositiveInfinity });

			_bounds = new BoundingSphere[objectCount];
			_cullingGroup.SetBoundingSpheres(_bounds);
			_cullingGroup.SetBoundingSphereCount(_length);

		}

		private void OnQuitting()
		{
			Application.quitting -= OnQuitting;

			if (_cullingGroup == null)
				return;

			_cullingGroup.Dispose();
			_cullingGroup = null;
		}

		protected void StateChanged(CullingGroupEvent e)
		{
			if (e.index >= _objects.Count)
				return;
			_objects[e.index].OnStateChange(e);
			OnCullingGroupEvent?.Invoke(e);
		}

		public int RegisterObject(CullingGroupTarget groupObject)
		{
			int index = -1;
			if (!_isInitialized)
			{
				Initialize();
			}
			if (!_objects.Contains(groupObject))
			{
				if (groupObject.IsDynamic && !DynamicCullingGroupSingleton.IsInstantiated)
				{
					DynamicCullingGroupSingleton.Instance.Profile = this;
				}
				_objects.Add(groupObject);
				OnAdded?.Invoke(groupObject);
				index = TryAdd(groupObject.Offset, groupObject.Radius);
			}
			return index;
		}

		private int TryAdd(Vector3 pos, float rad)
		{
			if (_cullingGroup == null)
			{
				return -1;
			}
			if (_length + 1 > objectCount)
			{
				return -1;
			}
			var b = new BoundingSphere
			{
				position = pos,
				radius = rad
			};
			int index = _length;
			_bounds[index] = b;
			_length++;
			_cullingGroup.SetBoundingSphereCount(_length);

			return index;
		}

		private bool TryRemoveAt(int index)
		{
			if (_cullingGroup == null)
			{
				return false;
			}
			if (index < 0 || index >= _length)
				return false;
			for (int i = index; i < _length; i++)
			{
				var next = i + 1;
				if (next >= _bounds.Length)
				{
					continue;
				}
				var other = _bounds[next];
				_bounds[i] = other;
			}
			_length--;
			_cullingGroup.SetBoundingSphereCount(_length);
			return true;
		}

		public void UnregisterObject(CullingGroupTarget groupObject)
		{
			if (!_objects.Contains(groupObject))
			{
				return;
			}
			var index = _objects.IndexOf(groupObject);
			onRemoved?.Invoke(index);
			_objects.Remove(groupObject);
			TryRemoveAt(index);
			RefreshIndex(index);
		}

		private void RefreshIndex(int start)
		{
			for (int i = start; i < _objects.Count; i++)
			{
				_objects[i].RefreshIndex(i);
			}
		}

		public void UpdateBounds(CullingGroupTarget groupObject)
		{
			var index = _objects.IndexOf(groupObject);
			UpdateBounds(index, groupObject.Offset, groupObject.Radius);

		}

		public void UpdateBounds(int index, Vector3 position, float radius)
		{
			if (index == -1 || _bounds == null)
			{
				return;
			}
			var bound = _bounds[index];
			bound.position = position;
			bound.radius = radius;
			_bounds[index] = bound;
		}

	}
}

