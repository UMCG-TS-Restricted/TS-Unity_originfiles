using UnityEngine;


namespace insitu
{
	/// <summary>
	///		This component is required as it is unknown what text rendering system is used.
	///		This can be for example TMP or Unity Text.
	/// </summary>
	/// <example>
	/// public class TextComponent : insitu.TextComponent
	/// {
	///		public TMP_Text Text;
	///		public override string text
	///		{
	///			get { return Text.text; }
	///			set { Text.text = value; }
	///		}
	///		public override Color color
	///		{
	///			get { return Text.color; }
	///			set { Text.color = value; }
	///		}
	///	}
	///	</example>
	public abstract class TextComponent : MonoBehaviour
	{
		public abstract string text { get; set; }
		public abstract Color color { get; set; }
	}
}
