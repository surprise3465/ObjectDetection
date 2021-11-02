using Android.Graphics;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ObjectDetection
{
    static class ImageService
    {
        static readonly HttpClient _client = new HttpClient();

        public static Task<byte[]> DownloadImage(string imageUrl)
        {
            if (!imageUrl.Trim().StartsWith("https", StringComparison.OrdinalIgnoreCase))
                throw new Exception("iOS and Android Require Https");

            return _client.GetByteArrayAsync(imageUrl);
        }

        public static void SaveToDisk(string imageFileName, byte[] imageAsBase64String)
        {
            Xamarin.Essentials.Preferences.Set(imageFileName, Convert.ToBase64String(imageAsBase64String));
        }

        public static Xamarin.Forms.ImageSource GetFromDisk(string imageFileName)
        {
            var imageAsBase64String = Xamarin.Essentials.Preferences.Get(imageFileName, string.Empty);

            return ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageAsBase64String)));
        }
    }

    public partial class MainPage : ContentPage
    {
        private string _uri = @"http://192.168.8.232/api/Storage/ana";
        private string _photoPath = "";
        public MainPage()
        {
            InitializeComponent();
        }

        private async void PredictButton_Clicked(object sender, EventArgs e)
        {
            var readWritePermission = DependencyService.Get<IReadWritePermission>();
            var status = await readWritePermission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await readWritePermission.RequestAsync();
            }
            if (status == PermissionStatus.Granted)
            {
                if (!CrossMedia.Current.IsCameraAvailable && !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", "No camera available.", "OK");
                    return;
                }

                var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Test",
                    SaveToAlbum = true,
                    CompressionQuality = 100,
                    CustomPhotoSize = 100,
                    PhotoSize = PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 2000,
                    DefaultCamera = CameraDevice.Front
                });

                if (photo != null)
                {
                    _photoPath = photo.Path;
                    
                    var httpClient = new HttpClient();
                    using (var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                    {
                        content.Add(new StreamContent(photo.GetStream()), "file", "upload.jpg");
                        try
                        {
                                using (var message = await httpClient.PostAsync(_uri, content))
                                {
                                    if (message.StatusCode == HttpStatusCode.OK)
                                    {
                                        var bytes = message.Content.ReadAsByteArrayAsync().Result;
                                        ImageService.SaveToDisk("upload.jpg", bytes);
                                        PhotoImage.Source = ImageService.GetFromDisk("upload.jpg");
                                    }
                                    else
                                    {
                                        await DisplayAlert("title", message.StatusCode.ToString(), "OK");
                                    }
                                }
                            }                 
                        catch (Exception exp)
                        {
                            await DisplayAlert("title", exp.ToString(), "OK");
                        } 
                    }
 
                }
            }
        }      
    }
}
