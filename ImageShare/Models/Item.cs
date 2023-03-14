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
	}
}
