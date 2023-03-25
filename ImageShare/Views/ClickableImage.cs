using System;
using ImageShare.Models;

namespace ImageShare
{
	public class ClickableImage
    {
		private static Stream imagePlaceholder;

		private Item Item;
		private StorageClient StorageClient;
		private ImageButton Button;
		
		public ClickableImage(Item item, StorageClient storageClient) : base()
		{
			StorageClient = storageClient;
			Item = item;
        }

		private INavigation Navigation;

		public void SetParentNavigation(INavigation navigation)
		{
			Navigation = navigation;
		}

		public View GetView()
		{
			// Use cached button if exists
			if (null != Button)
				return Button;

			// Create new view
			Button = new ImageButton()
			{
				// Set image placeholder
				Source = ImageSource.FromFile("image_placeholder.png")
			};
            Button.Clicked += DoImageClickedAction;


			return Button;
		}

		public void DownloadPreviewImage()
		{
			if (null == Button)
				return;
			// Download preview
			Stream imageStream = StorageClient.GetPreview(Item);
			ImageSource imageSource = ImageSource.FromStream(() => imageStream);
            // Replace image source
            Application.Current.Dispatcher.Dispatch(() =>
			{
				Button.Source = imageSource;
			});
        }

        private void DoImageClickedAction(object sender, EventArgs e)
        {
			if (null == Navigation)
				return;
            Navigation.PushAsync(new Views.FullscreenImage(Item, StorageClient));
        }
    }
}
