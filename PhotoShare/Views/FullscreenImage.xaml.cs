using Bertuzzi.MAUI.PinchZoomImage;
using PhotoShare.Models;

namespace PhotoShare.Views;

public partial class FullscreenImage : ContentPage
{
	public FullscreenImage(Item item, StorageClient storageClient)
	{
		InitializeComponent();
		Title = item.Name;

		Stream stream = storageClient.GetFullSize(item);

		PinchZoom pinchZoom = new PinchZoom()
		{
			Content = new Image()
			{
				Source = ImageSource.FromStream(() => stream),
			}
		};

		this.Content = pinchZoom;
	}
}
