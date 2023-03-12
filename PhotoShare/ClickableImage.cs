using System;
namespace PhotoShare
{
	public class ClickableImage : VerticalStackLayout
    {
		public ClickableImage(string name, string fileid, ImageFetcher imageFetcher)
		{
			
			Label label = new Label() { Text = name };
			this.Children.Add(label);
		}
	}
}
