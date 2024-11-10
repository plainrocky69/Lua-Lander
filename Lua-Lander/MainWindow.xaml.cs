using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NLua;
using ICSharpCode.AvalonEdit;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox; // Use WPF MessageBox


namespace LuaLander
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTab("Untitled.lua", string.Empty);
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Lua Files (*.lua)|*.lua|All Files (*.*)|*.*",
                Title = "Open Lua File"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileContent = File.ReadAllText(filePath);
                CreateNewTab(Path.GetFileName(filePath), fileContent, filePath);
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderPicker.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) // Correct comparison
            {
                string folderPath = folderPicker.SelectedPath;
                LoadFilesFromFolder(folderPath);
            }
        }

        private void LoadFilesFromFolder(string folderPath)
        {
            foreach (TabItem tab in TabControl.Items)
            {
                var editor = tab.Content as TextEditor;
                if (editor != null)
                {
                    editor.Text = string.Empty;
                }
            }

            string[] luaFiles = Directory.GetFiles(folderPath, "*.lua");
            foreach (var file in luaFiles)
            {
                string fileContent = File.ReadAllText(file);
                CreateNewTab(Path.GetFileName(file), fileContent, file);
            }
        }

        private void CreateNewTab(string title, string content, string filePath = null)
        {
            TabItem tabItem = new TabItem
            {
                Header = title
            };

            TextEditor editor = new TextEditor
            {
                Text = content,
                Tag = filePath,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("Lua"),
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
            };

            editor.Background = System.Windows.Media.Brushes.Black; // Dark background
            editor.Foreground = System.Windows.Media.Brushes.White;  // White text
            tabItem.Content = editor;
            TabControl.Items.Add(tabItem);
            TabControl.SelectedItem = tabItem;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabControl.Items.Count == 0)
            {
                MessageBox.Show("No files are open. Create or open a Lua file to run.", "Lua-Lander");
                return;
            }

            // Show the OutputConsole when the button is clicked
            OutputConsole.Visibility = Visibility.Visible;

            var selectedTab = TabControl.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var editor = selectedTab.Content as TextEditor;
                if (editor != null)
                {
                    RunLuaScript(editor.Text);
                }
            }
        }


        private void RunLuaScript(string luaCode)
        {
            OutputConsole.Clear();

            using (var lua = new Lua())
            {
                try
                {
                    var results = lua.DoString(luaCode);
                    OutputConsole.AppendText("Execution successful!\n");

                    if (results != null && results.Length > 0)
                    {
                        foreach (var result in results)
                        {
                            OutputConsole.AppendText(result?.ToString() + "\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    OutputConsole.AppendText($"Error: {ex.Message}\n");
                }
            }
        }
    }
}
