using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonManager : MonoBehaviour
{
	#region Fields

	[SerializeField]
	private TextMeshProUGUI label;

	private Button button;
	private UnityAction buttonAction;

	#endregion

	#region Public Methods

	public void Initialize(string label, string month, string year, Action<(string, string, string, string)> clickEventHandler)
	{
		this.label.text = label;

		buttonAction += () => clickEventHandler((label, month, year, label));
		button.onClick.AddListener(buttonAction);
	}

	#endregion

	#region Private Methods

	private void Awake()
	{
		button = GetComponent<Button>();
	}

	private void OnDestroy()
	{
		button.onClick.RemoveListener(buttonAction);
	}

	#endregion
}
