using TMPro;
using UnityEngine;


public class TextComponent : insitu.TextComponent
{
	public TMP_Text Text;

	public override string text
	{
		get => Text.text;
		set => Text.text = value;
	}

	public override Color color
	{
		get => Text.color;
		set => Text.color = value;
	}
}
