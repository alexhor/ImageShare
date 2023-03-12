using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;
using PhotoShare.Models;
using PhotoShare.Views;
using WebDav;

namespace PhotoShare;

public partial class MainPage : ContentPage
{
    protected StorageClient StorageClient;

    public MainPage()
    {
        StorageClient = new StorageClient(new Uri("https://cloud.h-software.de"), "t2tqiAFNewekiTq");

        InitializeComponent();

        // TODO: Open root folder page, if Nextcloud share link is stored
        Navigation.PushAsync(new FolderPage(new Folder(StorageClient)));

        // Load images
        //Task task = getPhotoList();
        /*
        
        */
    }   

    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        Navigation.PushAsync(new FolderPage(new Folder(StorageClient)));
    }
}


