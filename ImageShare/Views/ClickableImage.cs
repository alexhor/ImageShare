using System;
using ImageShare.Models;

namespace ImageShare
{
	public class ClickableImage
    {
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
            Stream imageStream = StorageClient.GetPreview(Item);
			Button = new ImageButton()
			{
				Source = ImageSource.FromStream(() => imageStream),
			};
            Button.Clicked += DoImageClickedAction;


			return Button;
		}

        private void DoImageClickedAction(object sender, EventArgs e)
        {
			if (null == Navigation)
				return;
            Navigation.PushAsync(new Views.FullscreenImage(Item, StorageClient));
        }
    }
}
