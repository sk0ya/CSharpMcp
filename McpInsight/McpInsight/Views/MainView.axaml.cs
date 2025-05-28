using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace McpInsight.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            
            // コントロールの初期化とイベントハンドラの設定はAttachedToVisualTreeイベントで行う
            this.AttachedToVisualTree += MainView_AttachedToVisualTree;
        }
        
        private void MainView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            // イベントハンドラを設定
            if (folderHistoryListBox != null)
            {
                folderHistoryListBox.SelectionChanged += FolderHistoryListBox_SelectionChanged;
            }
            
            if (argumentsHistoryListBox != null)
            {
                argumentsHistoryListBox.SelectionChanged += ArgumentsHistoryListBox_SelectionChanged;
            }
        }
        
        private void FolderHistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 項目が選択されたらFlyoutを閉じる
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                folderHistoryButton?.Flyout?.Hide();
            }
        }
        
        private void ArgumentsHistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 項目が選択されたらFlyoutを閉じる
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                argumentsHistoryButton?.Flyout?.Hide();
            }
        }
    }
}