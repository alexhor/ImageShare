using System;
namespace PhotoShare
{
	public class ImageFetcher
	{
		protected string sharingKey;

        public ImageFetcher(string sharingKey)
		{
			this.sharingKey = sharingKey;

        }

		public void getPreview(string filename, string fileid)
		{

		}
	}
}

