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

		private void Awake()
		{
			_target.transform = transform;
			_target.onStateChanged += OnStateChange;
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

	}
}

