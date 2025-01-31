using System;
using ADG;
using UnityEngine;


namespace insitu
{
	public abstract class GizmoBase : MonoBehaviour
	{
		[NonSerialized] public float GizmoAlpha;
		[NonSerialized] public float EditorAlpha;

		public bool ShowToPlayer;
		public bool ShowInEditor;
		public GizmoData Data;

		public void Update()
		{
			var deltaTime = Time.unscaledDeltaTime;
			var show_player = ShowToPlayer && Data;
			if (show_player)
			{
				GizmoAlpha += deltaTime / 0.4f;
				if (GizmoAlpha > 1)
					GizmoAlpha = 1;
			}
			else
			{
				GizmoAlpha -= deltaTime / 0.4f;
				if (GizmoAlpha < 0)
					GizmoAlpha = 0;
			}

			if (!show_player && ShowInEditor)
			{
				EditorAlpha += deltaTime / 0.4f;
				if (EditorAlpha > 1)
					EditorAlpha = 1;
			}
			else
			{
				EditorAlpha -= deltaTime / 0.4f;
				if (EditorAlpha < 0)
					EditorAlpha = 0;
			}

			if (Data)
			{
				var alpha = Ease.Hermite(GizmoAlpha);
				UpdatePlayerGizmo(alpha);
			}
		}

		public void OnDrawGizmos()
		{
			var alpha = Ease.Hermite(EditorAlpha);
			if (ShowInEditor && !Application.isPlaying)
				alpha = 1;

			UpdateEditorGizmo(alpha);
		}

		public abstract void UpdatePlayerGizmo(float alpha);
		public abstract void UpdateEditorGizmo(float alpha);
	}
}
