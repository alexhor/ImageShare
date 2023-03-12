using System;
namespace PhotoShare.Models
{
	public class Folder
	{
		public string Directory { get; protected set; }
		protected StorageClient StorageClient;

		public Folder(StorageClient storageClient, string directory = "")
		{
            Directory = directory;
            StorageClient = storageClient;
		}

		public async IAsyncEnumerable<ClickableImage> GetImages()
		{
			await foreach (Item item in StorageClient.GetChildList(this))
			{
                yield return new ClickableImage(item, StorageClient);
            }
		}
	}
}
