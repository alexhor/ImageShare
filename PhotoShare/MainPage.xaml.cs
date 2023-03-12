using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;
using WebDav;

namespace PhotoShare;

public partial class MainPage : ContentPage
{
    protected static IWebDavClient client;

    protected static ImageFetcher imageFetcher;

    protected static string sharingKey;


    public MainPage()
    {
        sharingKey = "t2tqiAFNewekiTq";

        WebDavClientParams clientParams = new WebDavClientParams
        {
            BaseAddress = new Uri("https://cloud.h-software.de/"),
            Credentials = new NetworkCredential(sharingKey, "")
        };
        client = new WebDavClient(clientParams);



        InitializeComponent();

        // Load images
        //Task task = getPhotoList();
    }

	private async Task getPhotoList()
	{
        var propfindParams = new PropfindParameters
        {
            RequestType = PropfindRequestType.NamedProperties,

            Namespaces = new[]
            {
                new NamespaceAttr("oc", "http://owncloud.org/ns"),
                new NamespaceAttr("nc", "http://nextcloud.org/ns"),
                new NamespaceAttr("ocs", "http://open-collaboration-services.org/ns"),
            },

            CustomProperties = new[]
            {
                XName.Get("fileid", "http://owncloud.org/ns"),
                XName.Get("displayname", "DAV:")
            }
        };
        
        var result = await client.Propfind("https://cloud.h-software.de/public.php/webdav/", propfindParams);
        if (result.IsSuccessful)
		{
            bool first = true;
            // Show images
            foreach (WebDavResource imageData in result.Resources)
            {
                // Show current folder on top
                if (first) {
                    first = false;
                    this.CurrentFolder.Text = imageData.DisplayName;
                    continue;
                }

                string fileid = "";
                foreach (var property in imageData.Properties)
                {

                }
                ClickableImage image = new ClickableImage(imageData.DisplayName, fileid, imageFetcher);
                Application.Current.Dispatcher.Dispatch(() =>
                {
                        this.Gallery.Children.Add(image);
                });
                //ImageListWrapper.Append(image);
            }

            //ImageList.CollectionChanged += ImageList_CollectionChanged;
        }
        else
		{
            // handle an error
		}
    }

    private void ImageList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }
}


