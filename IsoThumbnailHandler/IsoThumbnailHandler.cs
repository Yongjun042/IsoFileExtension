using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using DiscUtils.Udf;
using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;

namespace IsoThumbnailHandler
{
    /// <summary>
    /// The IsoThumbnailHandler is a ThumbnailHandler for text files.
    /// </summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".iso")]
    [Obsolete]
    public class IsoThumbnailHandler : SharpThumbnailHandler, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsoThumbnailHandler"/> class.
        /// </summary>
        public IsoThumbnailHandler()
        {
           // _renderer = new TextThumbnailRenderer();
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            if (width <= 0 ||
                height <= 0 ||
                image == null)
            {
                return null;
            }

            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.Clear(Color.White);
                graphics.DrawImage(image, 0, 0, width, height);
            }

            return destImage;
        }

        /// <summary>
        /// Gets the thumbnail image.
        /// </summary>
        /// <param name="width">The width of the image that should be returned.</param>
        /// <returns>
        /// The image for the thumbnail.
        /// </returns>
        protected override Bitmap GetThumbnailImage(uint width)
        {
            Log($"Creating thumbnail for '{SelectedItemStream.Name}'");

            //  Attempt to open the stream with a reader.
            try
            {
                //StreamReader reader = new StreamReader(SelectedItemStream);
                using (UdfReader cd = new UdfReader(SelectedItemStream))
                {
                    if (cd.Exists(@"BDMV\META\DL"))
                    {
                        Log($"DL Exist");
                        Bitmap thumbnail = null;
                        string[] files = cd.GetFiles(@"BDMV\META\DL", "*.jpg");
                        //find file that size is most similar and bigger than cx;
                        Log($"Foreach");
                        foreach (var item in files)
                        {
                            Log($"FileStream");
                            Stream fileStream = cd.OpenFile(item, FileMode.Open);
                            Log($"Bitmap");
                            Bitmap bmp = new Bitmap(fileStream);
                            if (thumbnail != null)
                            {
                                if ((width < Math.Max(thumbnail.Width, thumbnail.Height) && Math.Max(bmp.Width, bmp.Height) < Math.Max(thumbnail.Width, thumbnail.Height))
                                    || (width > Math.Max(thumbnail.Width, thumbnail.Height) && Math.Max(bmp.Width, bmp.Height) > Math.Max(thumbnail.Width, thumbnail.Height)))
                                {
                                    Log($"Selected '{item}'");
                                    thumbnail = bmp;
                                }
                            }
                            else
                            {
                                thumbnail = bmp;
                            }
                        }
                        if (thumbnail == null)
                        {
                            return null;
                        }
                        if (thumbnail.Width != width && thumbnail.Height != width)
                        {
                            Log($"Resizing");
                            // We are not the appropriate size for caller.  Resize now while
                            // respecting the aspect ratio.
                            float scale = Math.Min((float)width / thumbnail.Width, (float)width / thumbnail.Height);
                            int scaleWidth = (int)(thumbnail.Width * scale);
                            int scaleHeight = (int)(thumbnail.Height * scale);
                            thumbnail = ResizeImage(thumbnail, scaleWidth, scaleHeight);
                            Log($"Resize finished");
                        }
                        return thumbnail;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                //  DebugLog the exception and return null for failure.
                LogError("An exception occured opening the text file.", exception);
                return null;
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
           // _renderer?.Dispose();
        }

        /// <summary>The renderer used to create the text.</summary>
        //private readonly TextThumbnailRenderer _renderer;
    }
}
