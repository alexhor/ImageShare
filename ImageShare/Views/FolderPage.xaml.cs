using ImageShare.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace ImageShare.Views;

public partial class FolderPage : ContentPage
{
	protected Folder folder;

    private Queue<ClickableImage> PreviewsLeftToLoad = new Queue<ClickableImage>();

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
            View imageView = image.GetView();

            Application.Current.Dispatcher.Dispatch(() =>
            {
                Gallery.Children.Add(imageView);
            });
            PreviewsLeftToLoad.Enqueue(image);
        }

        // Stop indicating loading
        Application.Current.Dispatcher.Dispatch(() =>
        {
            activityIndicator.IsRunning = false;
            // This makes image buttons work that are initially located beyond the screen borders
            ScrollWrapper.VerticalOptions = LayoutOptions.Fill;
        });

        // Load previews
        Task task = LoadPreviews();
    }

    private async Task LoadPreviews()
    {
        // Continue with loading the image previews
        while (0 < PreviewsLeftToLoad.Count)
        {
            ClickableImage image = PreviewsLeftToLoad.Dequeue();
            Task task = Task.Run(() =>
            {
                image.DownloadPreviewImage();
            });
            await task.ConfigureAwait(true);
        }
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
