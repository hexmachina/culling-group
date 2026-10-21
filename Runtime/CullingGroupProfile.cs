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
		public event Action<CullingGroupTarget> OnTargetRemoved;
		public event Action<CullingGroupEvent> OnCullingGroupEvent;

		public delegate void ObjectsCallbackDelegate(List<CullingGroupTarget> objects);
		public delegate void ObjectCallbackDelegate(int index);
		[NonSerialized] private readonly Dictionary<EntityId, CullingGroupTarget> _targetDict = new();
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

		public IReadOnlyDictionary<EntityId, CullingGroupTarget> TargetDict => _targetDict;

		public Camera TargetCamera => _cullingGroup.targetCamera;

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
			if (_objects.Contains(groupObject))
			{
				return -1;
			}
			if (groupObject.IsDynamic && !DynamicCullingGroupSingleton.IsInstantiated)
			{
				DynamicCullingGroupSingleton.Instance.Profile = this;
			}
			index = TryAdd(groupObject.Offset, groupObject.Radius);
			if (index == -1)
			{
				Debug.LogWarning($"CullingGroupProfile: Failed to register object {groupObject}. Maximum object count reached.");
				return -1;
			}
			_targetDict[groupObject.ClaimId] = groupObject;
			_objects.Add(groupObject);
			OnAdded?.Invoke(groupObject);
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

		public void UnregisterObject(CullingGroupTarget groupObject)
		{

			int index = _objects.IndexOf(groupObject);
			if (index == -1)
			{
				return;
			}
			_objects.RemoveAt(index);
			_targetDict.Remove(groupObject.ClaimId);
			RefreshIndex(index);
			_length--;
			_cullingGroup?.SetBoundingSphereCount(_length);
			OnTargetRemoved?.Invoke(groupObject);
			onRemoved?.Invoke(index);
		}

		private void RefreshIndex(int start)
		{
			for (int i = start; i < _objects.Count; i++)
			{
				_objects[i].RefreshIndex(i);
				_objects[i].UpdateBounds();
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

