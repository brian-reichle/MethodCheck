// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Windows;
using System.Windows.Input;
using MethodCheck.Data;
using MethodCheck.Parsing;

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
			var blob = BinaryProcessor.Parse(BytesTextBox.Text);

			if (blob != null)
			{
				BytesTextBox.Text = BinaryProcessor.Format(blob);

				var data = Parse(blob);

				if (data != null)
				{
					ILTextBox.Text = MethodFormatter.Format(data);
				}
			}
		}

		MethodData Parse(byte[] blob)
		{
			if (SourceTypeIL.IsChecked == true)
			{
				return MethodParser.ParseIL(blob);
			}
			else if (SourceTypeBody.IsChecked == true)
			{
				return MethodParser.ParseBody(blob);
			}
			else
			{
				return null;
			}
		}
	}
}
