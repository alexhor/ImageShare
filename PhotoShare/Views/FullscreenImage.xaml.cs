using Bertuzzi.MAUI.PinchZoomImage;
using PhotoShare.Models;

namespace PhotoShare.Views;

public partial class FullscreenImage : ContentPage
{
	public FullscreenImage(Item item, StorageClient storageClient)
	{
		InitializeComponent();
		Title = item.Name;

		Task task = LoadImage(item, storageClient);
    }

	private async Task LoadImage(Item item, StorageClient storageClient)
    {
		Stream stream = await storageClient.GetFullSize(item);

        Image image = new Image()
		{
			Source = ImageSource.FromStream(() => stream),
		};


        PinchZoom pinchZoom = new PinchZoom()
		{
			Content = image,
        };



		//Wrapper.Children.Add(pinchZoom);
		Content = pinchZoom;
	}
}
