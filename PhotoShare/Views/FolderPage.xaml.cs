using PhotoShare.Models;

namespace PhotoShare.Views;

public partial class FolderPage : ContentPage
{
	protected Folder folder;

	public FolderPage(Folder folder)
	{
		this.folder = folder;

		InitializeComponent();

        // Load folder content
        Task task = LoadFolderContent();
    }

    public async Task LoadFolderContent()
	{
        await foreach (ClickableImage image in folder.GetImages())
        {
            image.SetParentNavigation(Navigation);
            Application.Current.Dispatcher.Dispatch(() =>
            {
                Gallery.Children.Add(image.GetView());
            });
        }

        // Stop indicating loading
        activityIndicator.IsRunning = false;
    }
}
