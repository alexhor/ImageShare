using System;
namespace ImageShare.Models
{
    public class Item
    {
        public Folder ParentFolder { get; private set; }

        public string Name { get; private set; }

        public string Fileid { get; private set; }

        public Item(Folder parentFolder, string name, string fileid)
        {
            Name = name;
            Fileid = fileid;
            ParentFolder = parentFolder;
        }

        /// <summary>
		/// Get the next image
		/// </summary>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public Item GetNextImage()
        {
            return ParentFolder.GetNextImage(this);
        }

        /// <summary>
        /// Get the previous image
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Item GetPreviousImage()
        {
            return ParentFolder.GetPreviousImage(this);
        }
    }
}
