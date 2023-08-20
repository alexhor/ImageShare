using ImageShare.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace ImageShare.Views;

public partial class FolderPage : ContentPage
{
	protected Folder folder;
    private List<ClickableImage> clickableImageList = new List<ClickableImage>();

    private Queue<ClickableImage> PreviewsLeftToLoad = new Queue<ClickableImage>();

	public FolderPage(Folder folder)
	{
		this.folder = folder;

		InitializeComponent();

        // Load folder content
        Task task = LoadFolderContent();
    }

    private async Task LoadFolderContent()
    {
        int clickableImageListIndex = 0;

        // Start indicating loading
        Application.Current.Dispatcher.Dispatch(() =>
        {
            activityIndicator.IsRunning = true;
        });

        await foreach (Item item in folder.GetImages())
        {
            int itemIndex;
            // If the end of the list was reached, add additional items at the end
            if (clickableImageListIndex >= clickableImageList.Count)
            {
                AddImage(item, clickableImageListIndex);
                clickableImageListIndex++;
            }
            // If this item exists at this position, do nothing
            else if (clickableImageList[clickableImageListIndex].Item.Equals(item))
            {
                clickableImageListIndex++;
            }
            // If this item exists later, delete from the list until the items position
            else if (-1 != (itemIndex = clickableImageList.FindIndex(i => i.Item.Equals(item))))
            {
                if (itemIndex <= clickableImageListIndex)
                    throw new IndexOutOfRangeException();
                for (int i = 0; i < itemIndex - clickableImageListIndex; i++)
                {
                    DeleteImage(clickableImageListIndex);
                }
                clickableImageListIndex++;
            }
            // If this item doesn't exist in the list, insert at current position
            else
            {
                AddImage(item, clickableImageListIndex);
                clickableImageListIndex++;
            }
        }

        // Delete from end of list
        while (clickableImageListIndex < clickableImageList.Count)
        {
            DeleteImage(clickableImageListIndex);
            clickableImageListIndex++;
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

        // TODO: Start regular update task
    }

    private void AddImage(Item item, int index)
    {
        ClickableImage image = new ClickableImage(item, this.folder.StorageClient);
        clickableImageList.Insert(index, image);
        image.SetParentNavigation(Navigation);
        View imageView = image.GetView();

        Application.Current.Dispatcher.Dispatch(() =>
        {
            Gallery.Children.Insert(index, imageView);
        });
        PreviewsLeftToLoad.Enqueue(image);
    }

    /// <summary>
    /// Delete image at index
    /// </summary>
    /// <param name="index"></param>
    private void DeleteImage(int index)
    {
        clickableImageList.RemoveAt(index);
        Application.Current.Dispatcher.Dispatch(() =>
        {
            Gallery.Children.RemoveAt(index);
        });
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
            try
            {
                await task.ConfigureAwait(true);
            }
            catch(AggregateException)
            {
                continue;
            }
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

                _ = LoadFolderContent();
            });
        });
    }

    void ReloadButton_Clicked(System.Object sender, System.EventArgs e)
    {
        _ = LoadFolderContent();
    }
}
