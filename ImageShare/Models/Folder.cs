using System;

namespace ImageShare.Models
{
	public class Folder
	{
		public string Directory { get; protected set; }
		protected StorageClient StorageClient;
		protected IList<Item> imageList = new List<Item>();

		public Folder(StorageClient storageClient, string directory = "")
		{
            Directory = directory;
            StorageClient = storageClient;
		}

		public async IAsyncEnumerable<ClickableImage> GetImages()
		{
			await foreach (Item item in StorageClient.GetChildList(this))
			{
				imageList.Add(item);
                yield return new ClickableImage(item, StorageClient);
            }
		}

		/// <summary>
		/// Get the next image
		/// </summary>
		/// <param name="currentImage"></param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public Item GetNextImage(Item currentImage)
		{
            int index = imageList.IndexOf(currentImage);
            return imageList[index + 1];
        }

		/// <summary>
		/// Get the previous image
		/// </summary>
		/// <param name="currentImage"></param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
        public Item GetPreviousImage(Item currentImage)
        {
			int index = imageList.IndexOf(currentImage);
			return imageList[index - 1];
        }

        public async Task<bool> UploadImage(string name, Stream content, string contentType)
		{
			return await StorageClient.UploadImage(this, name, content, contentType);
		}
	}
}
