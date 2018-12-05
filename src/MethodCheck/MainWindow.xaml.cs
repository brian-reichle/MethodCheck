// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Windows;
using System.Windows.Input;
using MethodCheck.Core;
using MethodCheck.Core.Data;
using MethodCheck.Core.Parsing;

namespace MethodCheck
{
	public partial class MainWindow : Window
	{
		public static readonly RoutedCommand Run = new RoutedCommand(nameof(Run), typeof(MainWindow));
		public static readonly RoutedCommand SetFocus = new RoutedCommand(nameof(SetFocus), typeof(MainWindow));

		public MainWindow()
		{
			InitializeComponent();

			Loaded += (sender, e) =>
			{
				var window = (MainWindow)sender;

				if (window.BytesTextBox != null)
				{
					window.BytesTextBox.Focus();
				}
			};
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			if (IsFocused && BytesTextBox != null)
			{
				BytesTextBox.Focus();
			}
		}

		void BytesTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			DoRun();
		}

		void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		void RunExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DoRun();
		}

		void SetFocusExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((FrameworkElement)e.Source).Focus();
		}

		void DoRun()
		{
			var buffer = BinaryProcessor.Parse(BytesTextBox.Text.AsSpan());

			if (buffer != null)
			{
				BytesTextBox.Text = BinaryProcessor.Format(buffer);

				var data = Parse(buffer);

				if (data != null)
				{
					ILTextBox.Text = MethodFormatter.Format(data);
				}
			}
		}

		MethodData Parse(ReadOnlySpan<byte> buffer)
		{
			if (SourceTypeIL.IsChecked == true)
			{
				return MethodParser.ParseIL(buffer);
			}
			else if (SourceTypeBody.IsChecked == true)
			{
				return MethodParser.ParseBody(buffer);
			}
			else
			{
				return null;
			}
		}
	}
}
