using Bertuzzi.MAUI.PinchZoomImage;
using ImageShare.Models;

namespace ImageShare.Views;

public partial class FullscreenImage : ContentPage
{
	protected StorageClient storageClient;
    protected Item item;

	public FullscreenImage(Item item, StorageClient storageClient)
	{
		InitializeComponent();
		Title = item.Name;
		this.storageClient = storageClient;
        this.item = item;

		Task task = LoadImage(item, storageClient);
    }

	private async Task LoadImage(Item item, StorageClient storageClient)
    {
		Stream stream = await storageClient.GetFullSize(item);

        Image image = new Image()
		{
			Source = ImageSource.FromStream(() => stream),
		};


        PinchZoom pinchZoom = new PinchZoom(this)
		{
			Content = image,
        };

		//Wrapper.Children.Add(pinchZoom);
		Content = pinchZoom;
	}

	public bool NextImage()
	{
		Item nextItem;

        try
		{
			nextItem = item.GetNextImage();
		}
		catch (IndexOutOfRangeException)
		{
			// If no next image exists, don't change the current image
			return false;
		}
		item = nextItem;
		Task task = LoadImage(nextItem, storageClient);
		return true;
    }

    public bool PreviousImage()
    {
		Item previousItem;

        try
		{
            previousItem = item.GetPreviousImage();
		}
		catch (IndexOutOfRangeException)
		{
			// If no previous image exists, don't change the current image
			return false;
		}
		item = previousItem;
		Task task = LoadImage(previousItem, storageClient);
		return true;
    }
}
