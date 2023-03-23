﻿using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using System.Xml.Linq;
using ImageShare.Models;
using ImageShare.Views;
using WebDav;

namespace ImageShare;

public partial class MainPage : ContentPage
{
    protected StorageClient StorageClient;

    public MainPage()
    {
        InitializeComponent();

        OpenFolder();
    }

    /// <summary>
    /// Open root folder page, if Nextcloud share link is stored
    /// </summary>
    public void OpenFolder()
    {
        // Try getting saved url
        string nextcloudShareUrl = Preferences.Default.Get("nextcloud_share_url", "");
        NextcloudSharingLink.Text = nextcloudShareUrl;

        if ("" == nextcloudShareUrl)
            return;

        // Parse sharing link
        string sharingKey = nextcloudShareUrl.Substring(nextcloudShareUrl.LastIndexOf('/') + 1);
        string pathWithoutSharingKey = nextcloudShareUrl.Substring(0, nextcloudShareUrl.LastIndexOf('/'));
        string rootUri = pathWithoutSharingKey.Substring(0, pathWithoutSharingKey.LastIndexOf('/'));

        // Open folder page
        StorageClient = new StorageClient(new Uri(rootUri), sharingKey);
        Navigation.PushAsync(new FolderPage(new Folder(StorageClient)));
    }

    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        Preferences.Default.Set("nextcloud_share_url", NextcloudSharingLink.Text);
        OpenFolder();
    }

    void OpenPrivacyPolicy(System.Object sender, System.EventArgs e)
    {
        string url = "https://h-software.de/privacy-policy.html";
        Launcher.OpenAsync(url);
    }
}


