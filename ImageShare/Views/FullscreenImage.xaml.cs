using Bertuzzi.MAUI.PinchZoomImage;
using CommunityToolkit.Maui.Converters;
using ImageShare.Models;
using Microsoft.Maui.Graphics.Platform;

namespace ImageShare.Views;

public partial class FullscreenImage : ContentPage
{
	protected StorageClient storageClient;
    protected Item item;
	private IDictionary<Item, Image> imageCache = new Dictionary<Item, Image>();

	public FullscreenImage(Item item, StorageClient storageClient)
	{
		InitializeComponent();
		Title = item.Name;
		this.storageClient = storageClient;
        this.item = item;
		ImageLoadingIcon.SetBinding(ActivityIndicator.IsRunningProperty, "IsLoading");

		LoadImage(item);
    }

	private void LoadImage(Item item)
    {
		Task.Run(async() => {
			Image image;

			if (imageCache.Keys.Contains(item))
			{
				image = imageCache[item];
			}
			else
			{
				// Show loading icon
				Dispatcher.Dispatch(() => {
					Content = new ActivityIndicator() {
						IsRunning = true,
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
					};
					Title = "Loading ...";
				});
				// Load full size image
				Stream stream = await storageClient.GetFullSize(item);
				StreamImageSource source = (StreamImageSource)ImageSource.FromStream(() => stream);
				image = new Image()
				{
					Source = source,
				};
				imageCache[item] = image;
			}

			Dispatcher.Dispatch(() =>
			{
				PinchZoom pinchZoom = new(this)
				{
					Content = image,
				};

				Content = pinchZoom;
				Title = item.Name;
				ImageLoadingIcon.BindingContext = image;
			});

			//UpdateImageCache(item);
		});
    }

	private void UpdateImageCache(Item item)
	{
		IList<Item> cacheItems = new List<Item>()
		{
			item
		};
		int itemIndex = item.ParentFolder.GetImageIndex(item);

		for (int i = itemIndex - 2; i <= itemIndex + 2; i++)
		{
			// Don't cache the already loaded image
			if (i == itemIndex)
			{
                continue;
			}

			Item itemToCache;
            try
            {
                itemToCache = item.ParentFolder.GetImageAt(i);
            }
            catch (IndexOutOfRangeException)
            {
				continue;
            }
			cacheItems.Add(itemToCache);

            if (imageCache.Keys.Contains(itemToCache))
			{
				continue;
			}

			
            Task<Stream> streamTask = storageClient.GetFullSize(itemToCache);
			streamTask.ContinueWith((stream) =>
			{
                Span<byte> memory = new Span<byte>();
                stream.Result.Read(memory);
                MemoryStream memoryStream = new MemoryStream(memory.ToArray());

                Image image = new Image()
                {
                    Source = ImageSource.FromStream(() => memoryStream),
                    //BindingContext = BindingContext,
                };

                imageCache[itemToCache] = image;
            });
        }

		/*
		foreach (Item cachedItem in imageCache.Keys)
		{
			if (!cacheItems.Contains(cachedItem))
			{
				imageCache.Remove(cachedItem);
			}
		}*/
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
		LoadImage(nextItem);
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
		LoadImage(previousItem);
		return true;
    }
}
