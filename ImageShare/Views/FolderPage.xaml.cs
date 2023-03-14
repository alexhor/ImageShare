using ImageShare.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace ImageShare.Views;

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

    void TakePicture(System.Object sender, System.EventArgs e)
    {
        Task<FileResult> photoResult = MediaPicker.Default.CapturePhotoAsync();

        photoResult.ContinueWith((task) =>
        {
            // Catch taking photo was cancled
            if (null == task.Result)
                return;

            // Show user that upload is in progress
            ISnackbar snackbar = Snackbar.Make("Uploading photo ...", duration: TimeSpan.MaxValue);
            Application.Current.Dispatcher.Dispatch(() =>
            {
                snackbar.Show();
            });

            // Upload file
            string fileName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Task<Stream> photoStream = task.Result.OpenReadAsync();
            photoStream.Wait();

            Task<bool> uploadTask = folder.UploadImage(fileName, photoStream.Result, task.Result.ContentType);

            // Show message when file was uploaded
            uploadTask.ContinueWith((task) =>
            {
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    snackbar.Dismiss();
                });
                string text = task.Result ? "Photo uploaded" : "Photo uploading failed";
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    var toast = Toast.Make(text, duration, fontSize);
                    toast.Show();
                });
            });
        });
    }
}
