﻿using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Xml.Linq;
using ImageShare.Models;
using SkiaSharp;
using WebDav;

namespace ImageShare
{
	public class StorageClient
	{
        private IWebDavClient WebdavClient;

        private HttpClient HttpClient;

        protected string SharingKey;

        private Uri ServerBaseUri;

        protected Uri WebdavUri
        { get
            {
                return new Uri(ServerBaseUri, "/public.php/webdav/");
            }
        }

        protected UriBuilder PublicPreviewUriBuilder
        { get
            {
                return new UriBuilder(new Uri(ServerBaseUri, "/apps/files_sharing/publicpreview/" + SharingKey + "?x=100&y=100"));
            }
        }

        /// <summary>
        /// Webdav parameters
        /// </summary>
        private static PropfindParameters propfindParams = new PropfindParameters
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
                    XName.Get("displayname", "DAV:"),
                    XName.Get("getlastmodified", "DAV:"),
                }
        };

        /// <summary>
        /// Client to interface with shared nextcloud folder
        /// </summary>
        /// <param name="baseAddress">Uri of Nextcloud instance</param>
        /// <param name="sharingKey">Key for shared Nextcloud folder</param>
        public StorageClient(Uri baseAddress, string sharingKey)
		{
            SharingKey = sharingKey;
            ServerBaseUri = baseAddress;

            SocketsHttpHandler handler = new SocketsHttpHandler
            {
                Credentials = new NetworkCredential(SharingKey, ""),
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            };

            HttpClient = new HttpClient(handler, true)
            {
                BaseAddress = ServerBaseUri,
            };

            WebdavClient = new WebDavClient(HttpClient);
        }

        /// <summary>
        /// Get the preview image of an item
        /// </summary>
        /// <param name="item">The item to get the preview of</param>
		public Stream GetPreview(Item item)
		{
            UriBuilder uriBuilder = PublicPreviewUriBuilder;
            // Load any query parameters currently attached to the preview uri
            NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["file"] = item.Name;
            query["fileid"] = item.Fileid;
            uriBuilder.Query = query.ToString();

            Task<Stream> getStreamTask = HttpClient.GetStreamAsync(uriBuilder.Uri);
            getStreamTask.Wait();
            return getStreamTask.Result;
		}

        /// <summary>
        /// Get the image in full size
        /// </summary>
        /// <param name="item">Image to fetch</param>
        /// <returns>Full size stream of the image</returns>
        public async Task<Stream> GetFullSize(Item item)
        {
            Uri uri = new Uri(new Uri(WebdavUri, item.ParentFolder.Directory), item.Name);
            WebDavStreamResponse response = await WebdavClient.GetRawFile(uri);
            return response.Stream;
        }

        /// <summary>
        /// Get all child items of a folder
        /// </summary>
        /// <param name="parentFolder">The folder to fetch children for</param>
        /// <returns>All items in this folder</returns>
        public async IAsyncEnumerable<Item> GetChildList(Folder parentFolder)
        {
            PropfindResponse result;
            try
            {
                result = await WebdavClient.Propfind(WebdavUri, propfindParams);
            }
            catch
            {
                // TODO: notify user about failed loading
                yield break;
            }
            
            if (result.IsSuccessful)
            {
                // Skip current folder
                IEnumerable<WebDavResource> imageList = result.Resources.Skip(1).Reverse();

                // Show images
                foreach (WebDavResource imageData in imageList)
                {
                    string fileid = "";
                    foreach (var property in imageData.Properties)
                    {
                        if ("fileid" == property.Name.LocalName)
                            fileid = property.Value;
                    }

                    yield return new Item(parentFolder, imageData.DisplayName, fileid);
                }
            }
            else
            {
                // TODO: log error
            }
            // Stop yield
            yield break;
        }

        /// <summary>
        /// Upload an image to a folder
        /// </summary>
        /// <param name="folder">Folder to upload to</param>
        /// <param name="filename">Name of uploaded file</param>
        /// <param name="fileContent">Content of file to upload</param>
        /// <param name="contentType">Content type of file</param>
        /// <returns>Success or failure</returns>
        public async Task<bool> UploadImage(Folder folder, string filename, Stream fileContent, string contentType)
        {
            filename += ".jpeg";
            Stream compressedContent = StorageClient.CompressImageStream(fileContent);

            Uri fileUri = new Uri(new Uri(WebdavUri, folder.Directory), filename);
            WebDavResponse response = await WebdavClient.PutFile(fileUri, compressedContent);
            return response.IsSuccessful;
        }

        public static Stream CompressImageStream(Stream stream, int quality=85)
        {
            SKBitmap sourceBitmap = SKBitmap.Decode(stream);
            SKImage scaledImage = SKImage.FromBitmap(sourceBitmap);
            SKData imageData = scaledImage.Encode(SKEncodedImageFormat.Jpeg, quality);

            return imageData.AsStream(true);
        }
    }
}
