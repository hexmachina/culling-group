using UnityEngine;
using UnityEngine.Events;

namespace TW.CullingGroups
{
	public class CullingGroupObject : MonoBehaviour
	{

		[System.Serializable]
		public class UnityCullingGroupEvent : UnityEvent<CullingGroupEvent> { }

		[System.Serializable]
		public class UnityStatusEvent : UnityEvent<bool, int> { }

		[SerializeField] protected CullingGroupTarget _target = new();

		public CullingGroupTarget Target => _target;

		public Vector3 Center { get => _target.Center; set => _target.Center = value; }

		public float Radius { get => _target.Radius; set => _target.Radius = value; }

		public Vector3 Offset => transform.TransformPoint(_target.Center);

		public UnityCullingGroupEvent onCullingStateChange = new();

		public bool IsVisible { get; private set; }

		public int Distance { get; set; }

		public CullingGroupEvent Current => _target.Current;

		private void Awake()
		{
			_target.transform = transform;
			_target.OnStateChanged += OnStateChange;
		}

		private void OnEnable()
		{
			_target.Register();
		}

		private void OnDisable()
		{
			_target.Unregister();
		}

		public virtual void UpdateBounds()
		{
			_target.UpdateBounds();
		}

		public virtual void OnStateChange(CullingGroupEvent e)
		{
			IsVisible = e.isVisible;
			Distance = e.currentDistance;
			if (!isActiveAndEnabled)
			{
				return;
			}
			onCullingStateChange.Invoke(e);
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying && isActiveAndEnabled)
			{
				if (_target.IsDynamic)
				{
					var pos = _target.GetPoint();
					Gizmos.color = Color.cyan;
					Gizmos.DrawRay(pos - Vector3.up * 0.5f, Vector3.up * 0.5f);
					Gizmos.DrawRay(pos - Vector3.right * 0.5f, Vector3.right);
					Gizmos.DrawRay(pos - Vector3.forward * 0.5f, Vector3.forward);
				}
#if UNITY_EDITOR
				UnityEditor.Handles.color = Color.yellow;
				UnityEditor.Handles.Label(Offset, $"{_target.Distance}\n{_target.Current.isVisible}");
#endif
			}
		}

	}
}

