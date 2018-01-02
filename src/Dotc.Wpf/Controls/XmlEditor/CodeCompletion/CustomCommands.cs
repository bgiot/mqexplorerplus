


using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
	/// <summary>
	/// Custom commands for CodeEditor.
	/// </summary>
	public static class CustomCommands
	{
		public static readonly RoutedCommand CtrlSpaceCompletion = new RoutedCommand(
			"CtrlSpaceCompletion", typeof(TextEditor),
			new InputGestureCollection {
				new KeyGesture(Key.Space, ModifierKeys.Control)
			});
	}
}
