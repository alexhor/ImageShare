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

    void ToggleAddItemMenu(System.Object sender, System.EventArgs e)
    {
        AddItemMenu.IsVisible = !AddItemMenu.IsVisible;
    }

    void TakePicture(System.Object sender, System.EventArgs e)
    {
        // TODO: add permissions request for Android
        Task<FileResult> photoResult = MediaPicker.Default.CapturePhotoAsync();

        photoResult.ContinueWith((task) =>
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Task<Stream> photoStream = task.Result.OpenReadAsync();
            photoStream.Wait();
            fileName += Path.GetExtension(task.Result.FileName);
            Task uploadTask = folder.UploadImage(fileName, photoStream.Result, task.Result.ContentType);
        });
    }

    void SelectFromLibrary(System.Object sender, System.EventArgs e)
    {
        // TODO: add permissions request for Android
    }
}
