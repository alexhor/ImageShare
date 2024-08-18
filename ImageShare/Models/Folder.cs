using System;

namespace ImageShare.Models
{
	public class Folder
	{
		public string Directory { get; protected set; }
		public StorageClient StorageClient;
		protected List<Item> imageList = new List<Item>();

		public Folder(StorageClient storageClient, string directory = "")
		{
            Directory = directory;
            StorageClient = storageClient;
		}

		public async IAsyncEnumerable<Item> GetImages()
		{
			imageList.Clear();
            await foreach (Item item in StorageClient.GetChildList(this))
			{
				imageList.Add(item);
				yield return item;
            }
		}

		public Item GetImageAt(int index)
		{
			return imageList[index];
		}

		public int GetImageIndex(Item item)
		{
			return imageList.IndexOf(item);
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
