using Microsoft.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace MyHelpers.Images
{
    public class MyImages
    {

        //FREDDY

        #region Contructores
        /// <summary>
        /// Constructor Default
        /// </summary>
        public MyImages() { }
        #endregion

        /// <summary>
        /// Obtiene un control tipo imagen con las medidas justas para que no quede desproporcionada en un area dada.
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Url"></param>
        /// <param name="File"></param>
        /// <returns></returns>
        public static System.Web.UI.WebControls.Image GetFixedImage(int Width, int Height, FileInfo File)
        {
            /* La idea es la siguiente: Por que numero multiplique la medida original para que llegara a la que yo quiero. Por ese
             * mismo numero multiplico la otra medida. Luego si aun no cabe hago la misma operacion con la otra medida. */
            System.Web.UI.WebControls.Image ret;
            System.Drawing.Image imgInput = System.Drawing.Image.FromFile(File.FullName);
            double newWidth = 0;
            double newHeight = 0;

            //get image original width and height
            int ImageWidth = imgInput.Width;
            int ImageHeight = imgInput.Height;

            //variable coeficiente.
            double dblCoef;


            // 1.- Cabe perfecto
            if (ImageWidth <= Width && ImageHeight <= Height)
            {
                newWidth = ImageWidth;
                newHeight = ImageHeight;
            }
            else
            {
                // escalamos por el width primero
                if (ImageWidth > Width)
                {
                    dblCoef = Width / (double)ImageWidth;
                    newWidth = ImageWidth * dblCoef;
                    newHeight = ImageHeight * dblCoef;
                    //con esto ya aseguramos que cabe el widht
                }
                else
                {
                    newWidth = ImageWidth;
                    newHeight = ImageHeight;
                }

                // si el height resultante no cabe, ahora escalamos por el height
                if (newHeight > Height)
                {
                    dblCoef = Height / (double)newHeight;
                    newWidth = newWidth * dblCoef;
                    newHeight = newHeight * dblCoef;
                }
            }

            ret = new System.Web.UI.WebControls.Image();
            ret.Width = (Unit)newWidth;
            ret.Height = (Unit)newHeight;

            imgInput.Dispose();

            return ret;
        }

        /// <summary>
        /// Obtiene un control tipo imagen con las medidas justas para que no quede desproporcionada en un area dada.
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Url"></param>
        /// <param name="File"></param>
        /// <returns></returns>
        public static void GetFixedImage(byte[] byteArrayOriginal, int Width, int Height, out byte[] byteArrayFinal)
        {
            /* La idea es la siguiente: Por que numero multiplique la medida original para que llegara a la que yo quiero. Por ese
             * mismo numero multiplico la otra medida. Luego si aun no cabe hago la misma operacion con la otra medida. */

            System.Drawing.Image imgInput = System.Drawing.Image.FromStream(new MemoryStream(byteArrayOriginal));
            double newWidth = 0;
            double newHeight = 0;

            //get image original width and height
            int ImageWidth = imgInput.Width;
            int ImageHeight = imgInput.Height;

            //variable coeficiente.
            double dblCoef;


            // 1.- Cabe perfecto
            if (ImageWidth <= Width && ImageHeight <= Height)
            {
                newWidth = ImageWidth;
                newHeight = ImageHeight;
            }
            else
            {
                // escalamos por el width primero
                if (ImageWidth > Width)
                {
                    dblCoef = Width / (double)ImageWidth;
                    newWidth = ImageWidth * dblCoef;
                    newHeight = ImageHeight * dblCoef;
                    //con esto ya aseguramos que cabe el widht
                }
                else
                {
                    newWidth = ImageWidth;
                    newHeight = ImageHeight;
                }

                // si el height resultante no cabe, ahora escalamos por el height
                if (newHeight > Height)
                {
                    dblCoef = Height / (double)newHeight;
                    newWidth = newWidth * dblCoef;
                    newHeight = newHeight * dblCoef;
                }
            }


            Bitmap bmPhoto = new Bitmap((int)newWidth, (int)newHeight, PixelFormat.Format24bppRgb);
            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            grPhoto.DrawImage(imgInput, new Rectangle(0, 0, (int)newWidth, (int)newHeight), new Rectangle(0, 0, (int)ImageWidth, (int)ImageHeight), GraphicsUnit.Pixel);

            MemoryStream mm = new MemoryStream();
            bmPhoto.Save(mm, System.Drawing.Imaging.ImageFormat.Jpeg);

            byteArrayFinal = mm.ToArray();

            bmPhoto.Dispose();
            grPhoto.Dispose();

            //ret = new System.Web.UI.WebControls.Image();
            //ret.Width = (Unit)newWidth;
            //ret.Height = (Unit)newHeight;

            imgInput.Dispose();

        }

        /// <summary>
        /// Elimina todos los archivos de imagen que no tengan la palabra "thumbnail" en el nombre de archivo
        /// </summary>
        /// <param name="Directory"></param>
        public static void DeleteNonThumbnailFiles(string Directory, HttpServerUtility Server)
        {
            // obtengo el directorio
            DirectoryInfo dir = new DirectoryInfo(Server.MapPath(Directory));

            // obtengo los archivos del directorio
            FileInfo[] deleteFiles = dir.GetFiles();
            foreach (FileInfo file in deleteFiles)
            {
                // obtengo el nombre del archivo
                String fileName = file.Name.ToLower();

                // si no tiene la palabra "thumbnail" elimina el archivo
                if (fileName.IndexOf("thumbnail") <= 0)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Genera un thumbnail por cada imagen en un directorio
        /// </summary>
        /// <param name="FolderName"></param>
        /// <param name="Server"></param>
        public static void GenerateThumbNailImagesForFolder(string FolderName, HttpServerUtility Server, Size ThumbNailSize)
        {
            // algunas variables
            string physicalPath = "";
            string fileName = "";
            string thumbName = "";

            // obtengo el path del servidos
            physicalPath = Server.MapPath(FolderName);

            // obtengo el directorio
            DirectoryInfo directorio = new DirectoryInfo(physicalPath);

            try
            {
                // respaldo
                FileInfo[] deleteFiles = directorio.GetFiles();
                FileInfo[] files = directorio.GetFiles();

                foreach (FileInfo file in files)
                {
                    // obtengo el nombre del archivo
                    fileName = file.Name.ToLower();

                    // si no tiene la palabra thumbnail
                    if (fileName.IndexOf("thumbnail") <= 0)
                    {
                        // cambio "nombre.extension" por "nombre_thumbnail.extension"
                        thumbName = fileName.Replace(".", "_thumbnail.");

                        // obtengo las medidas adecuadas para el thumb
                        System.Web.UI.WebControls.Image fixedSize = GetFixedImage(ThumbNailSize.Width, ThumbNailSize.Height, file);

                        // evaluo la extension del archivo
                        if (fileName.IndexOf(".gif") > 0)
                        {
                            GenerateThumbNail(physicalPath, fileName, thumbName, ImageFormat.Gif, new System.Drawing.Size(Convert.ToInt32(fixedSize.Width.Value), Convert.ToInt32(fixedSize.Height.Value)));
                        }
                        if (fileName.IndexOf(".jpg") > 0)
                        {
                            GenerateThumbNail(physicalPath, fileName, thumbName, ImageFormat.Jpeg, new System.Drawing.Size(Convert.ToInt32(fixedSize.Width.Value), Convert.ToInt32(fixedSize.Height.Value)));
                        }
                        if (fileName.IndexOf(".bmp") > 0)
                        {
                            GenerateThumbNail(physicalPath, fileName, thumbName, ImageFormat.Bmp, new System.Drawing.Size(Convert.ToInt32(fixedSize.Width.Value), Convert.ToInt32(fixedSize.Height.Value)));
                        }
                        fixedSize.Dispose();


                    }
                }
            }
            catch (Exception) { }
        }

        public static void GenerateThumbNailImagesForFolder(string FolderNameFisico, Size ThumbNailSize)
        {
            // algunas variables
            string physicalPath = "";
            string fileName = "";
            string thumbName = "";

            // obtengo el path del servidos
            physicalPath = FolderNameFisico + @"\thumbs";

            // obtengo el directorio
            DirectoryInfo directorio = new DirectoryInfo(physicalPath);

            try
            {
                // respaldo
                FileInfo[] deleteFiles = directorio.GetFiles();
                FileInfo[] files = directorio.GetFiles();

                foreach (FileInfo file in files)
                {
                    // obtengo el nombre del archivo
                    fileName = file.Name.ToLower();

                    // si no tiene la palabra thumbnail
                    if (fileName.IndexOf("thumbnail") <= 0)
                    {
                        // cambio "nombre.extension" por "nombre_thumbnail.extension"
                        thumbName = fileName.Replace(".", "_thumbnail.");

                        // obtengo las medidas adecuadas para el thumb
                        System.Web.UI.WebControls.Image fixedSize = GetFixedImage(ThumbNailSize.Width, ThumbNailSize.Height, file);

                        // evaluo la extension del archivo
                        if (fileName.IndexOf(".gif") > 0)
                        {
                            GenerateThumbNail(physicalPath, fileName, thumbName, ImageFormat.Gif, new System.Drawing.Size(Convert.ToInt32(fixedSize.Width.Value), Convert.ToInt32(fixedSize.Height.Value)));
                        }
                        if (fileName.IndexOf(".jpg") > 0)
                        {
                            GenerateThumbNail(physicalPath, fileName, thumbName, ImageFormat.Jpeg, new System.Drawing.Size(Convert.ToInt32(fixedSize.Width.Value), Convert.ToInt32(fixedSize.Height.Value)));
                        }
                        if (fileName.IndexOf(".bmp") > 0)
                        {
                            GenerateThumbNail(physicalPath, fileName, thumbName, ImageFormat.Bmp, new System.Drawing.Size(Convert.ToInt32(fixedSize.Width.Value), Convert.ToInt32(fixedSize.Height.Value)));
                        }

                        if (fileName.IndexOf(".png") > 0)
                        {
                            GenerateThumbNail(physicalPath, fileName, thumbName, ImageFormat.Png, new System.Drawing.Size(Convert.ToInt32(fixedSize.Width.Value), Convert.ToInt32(fixedSize.Height.Value)));
                        }
                        fixedSize.Dispose();


                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Genera un thumbnail de la imagen especifica con las medidas adecuadas
        /// </summary>
        /// <param name="PhysicalPath"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="ThumbNailFileName"></param>
        /// <param name="Format"></param>        
        public static void GenerateThumbNail(string PhysicalPath, string OriginalFileName, string ThumbNailFileName, ImageFormat Format, Size ThumbNailSize)
        {
            try
            {
                // obtengo la imagen del archivo
                System.Drawing.Image img = System.Drawing.Image.FromFile(PhysicalPath + @"\" + OriginalFileName);

                // creo el archivo thumbnail
                System.Drawing.Image thumbNail = new Bitmap(ThumbNailSize.Width, ThumbNailSize.Height, img.PixelFormat);

                // ajusto la calidad de imagen
                Graphics graphic = Graphics.FromImage(thumbNail);
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // creo que esto tiene algo que ver con el bounding box
                Rectangle rectangle = new Rectangle(0, 0, ThumbNailSize.Width, ThumbNailSize.Height);

                // render
                graphic.DrawImage(img, rectangle);

                // save
                thumbNail.Save(PhysicalPath + @"\" + ThumbNailFileName, Format);

                // liberamos recursos
                img.Dispose();
                thumbNail.Dispose();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Elimina todos los archivos de imagen que no tengan la palabra "thumbnail" en el nombre de archivo
        /// </summary>
        /// <param name="Directory"></param>
        public static void DeleteThumbnailFiles(string Directory, HttpServerUtility Server)
        {
            // obtengo el directorio
            DirectoryInfo dir = new DirectoryInfo(Server.MapPath(Directory));

            // obtengo los archivos del directorio
            FileInfo[] deleteFiles = dir.GetFiles();
            foreach (FileInfo file in deleteFiles)
            {
                // obtengo el nombre del archivo
                String fileName = file.Name.ToLower();

                // si tiene la palabra "thumbnail" elimina el archivo
                if (fileName.IndexOf("thumbnail") >= 0)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Cropea una imagen tomando en cuenta sus dimensiones. Salva la imagen cropeada en el mismo directorio de la original con el prefijo "Crop_"
        /// </summary>
        /// <param name="PhysicalPath"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="CropWidth"></param>
        /// <param name="CropHeight"></param>
        public static void CropImage(string PhysicalPath, string OriginalFileName, int CropWidth, int CropHeight)
        {
            int X = 0;
            int Y = 0;
            System.Drawing.Bitmap bmp;

            try
            {
                // obtengo la imagen del archivo
                System.Drawing.Image img = System.Drawing.Image.FromFile(PhysicalPath + @"\" + OriginalFileName);

                // obtengo el tamaño original de la imagen
                int OriginalWidth = img.Width;
                int OriginalHeight = img.Height;

                // 1.- 
                if (OriginalWidth <= CropWidth && OriginalHeight <= CropHeight)
                {
                    bmp = new Bitmap(OriginalWidth, OriginalHeight, PixelFormat.Format24bppRgb);

                } // 2.- 
                else if (OriginalWidth > CropWidth && OriginalHeight <= CropHeight)
                {
                    X = (OriginalWidth / 2) - (CropWidth / 2);
                    bmp = new Bitmap(CropWidth, OriginalHeight, PixelFormat.Format24bppRgb);
                } // 3.-
                else if (OriginalWidth <= CropWidth && OriginalHeight > CropHeight)
                {
                    Y = (OriginalHeight / 2) - (CropHeight / 2);
                    bmp = new Bitmap(OriginalWidth, CropHeight, PixelFormat.Format24bppRgb);
                } // 4.-
                else
                {
                    X = (OriginalWidth / 2) - (CropWidth / 2);
                    Y = (OriginalHeight / 2) - (CropHeight / 2);
                    bmp = new Bitmap(CropWidth, CropHeight, PixelFormat.Format24bppRgb);
                }

                // construimos el objeto Graphics con el que se trabaja
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.White);

                // construimos el canvas y el crop
                Rectangle canvas = new Rectangle(0, 0, bmp.Width, bmp.Height);
                Rectangle crop = new Rectangle(X, Y, bmp.Width, bmp.Height);

                // dibujamos y salvamos en el servidor
                g.DrawImage(img, canvas, crop, GraphicsUnit.Pixel);
                bmp.Save(PhysicalPath + @"\Crop_" + OriginalFileName, ImageFormat.Jpeg);

                // liberamos recursos
                g.Dispose();
                img.Dispose();
                bmp.Dispose();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Cropea una imagen tomando en cuenta sus dimensiones. Devuelve la imagen cropeada por un Arreglo de Byte lista para mostrar o almacenar en BD.
        /// </summary>
        /// <param name="imagen"></param>
        /// <param name="CropWidth"></param>
        /// <param name="CropHeight"></param>
        /// <param name="CropImage"></param>
        public static void CropImage(byte[] imagen, int CropWidth, int CropHeight, out byte[] CropImage)
        {
            int X = 0;
            int Y = 0;
            System.Drawing.Bitmap bmp;

            try
            {
                // obtengo la imagen del arreglo de byte


                System.Drawing.Image img = System.Drawing.Image.FromStream(new MemoryStream(imagen));

                // obtengo el tamaño original de la imagen
                int OriginalWidth = img.Width;
                int OriginalHeight = img.Height;

                // 1.- 
                if (OriginalWidth <= CropWidth && OriginalHeight <= CropHeight)
                {
                    bmp = new Bitmap(OriginalWidth, OriginalHeight, PixelFormat.Format24bppRgb);

                } // 2.- 
                else if (OriginalWidth > CropWidth && OriginalHeight <= CropHeight)
                {
                    X = (OriginalWidth / 2) - (CropWidth / 2);
                    bmp = new Bitmap(CropWidth, OriginalHeight, PixelFormat.Format24bppRgb);
                } // 3.-
                else if (OriginalWidth <= CropWidth && OriginalHeight > CropHeight)
                {
                    Y = (OriginalHeight / 2) - (CropHeight / 2);
                    bmp = new Bitmap(OriginalWidth, CropHeight, PixelFormat.Format24bppRgb);
                } // 4.-
                else
                {
                    X = (OriginalWidth / 2) - (CropWidth / 2);
                    Y = (OriginalHeight / 2) - (CropHeight / 2);
                    bmp = new Bitmap(CropWidth, CropHeight, PixelFormat.Format24bppRgb);
                }

                // construimos el objeto Graphics con el que se trabaja
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.White);

                // construimos el canvas y el crop
                Rectangle canvas = new Rectangle(0, 0, bmp.Width, bmp.Height);
                Rectangle crop = new Rectangle(X, Y, bmp.Width, bmp.Height);

                // dibujamos y salvamos en el servidor
                g.DrawImage(img, canvas, crop, GraphicsUnit.Pixel);
                //                bmp.Save(PhysicalPath + @"\Crop_" + OriginalFileName, ImageFormat.Jpeg);

                MemoryStream mstream = new MemoryStream();
                bmp.Save(mstream, ImageFormat.Jpeg);
                CropImage = mstream.ToArray();

                // liberamos recursos
                g.Dispose();
                img.Dispose();
                bmp.Dispose();
            }
            catch (Exception)
            {
                throw;
            }
        }


        //ALEXYOMAR

        public enum FixedSizeOpt
        {
            FixedBothSides,
            FixedWidthCropHeight,
            FixedHeightCropWidth
        }

        private enum ResizeType
        {
            PixelFixed,
            PixelMax,
            Percent
        }

        
        public static void SaveJpeg(FileUpload UploadControl, string destino, int quality)
        {

            if (UploadControl.PostedFile.ContentLength > 5242880)
            {
                throw new ArgumentOutOfRangeException("La imagen es mayor a 5 MB: " + UploadControl.PostedFile.FileName);
            }
            else
            {
                System.Drawing.Image curBitmap = Bitmap.FromStream(UploadControl.PostedFile.InputStream);


                if (curBitmap.Width > 600)
                {
                    curBitmap = ToMaxSize(curBitmap, 600, 600);
                }

                if (quality < 0 || quality > 100)
                {
                    throw new ArgumentOutOfRangeException("quality debe estar entre 0 y 100.");

                }

                // Encoder parameter for image quality
                EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                // Jpeg image codec
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;

                curBitmap.Save(destino, jpegCodec, encoderParams);
                curBitmap.Dispose();

            }


        }

        /// <summary>
        /// Salva un archivo de imagen en un directorio especificado
        /// </summary>
        /// <param name="UploadControl">FileUpload con el archivo a almacenar</param>
        /// <param name="destino">Ruta destino</param>
        /// <param name="quality">Calidad de Imagen (de 0 a 100)</param>
        /// <param name="MaxSize">Tamaño maximo en Width y Height de la imagen (canvas)</param>
        /// <param name="Width">Width resultado</param>
        /// <param name="Height">Height resultado</param>
        public static void SaveJpegNew(FileUpload UploadControl, string destino, int quality, int MaxSize, out int Width, out int Height)
        {               
            if (UploadControl.PostedFile.ContentLength > 5242880)
                throw new ArgumentOutOfRangeException("La imagen es mayor a 5 MB: " + UploadControl.PostedFile.FileName);
            else
            {
                System.Drawing.Image curBitmap = Bitmap.FromStream(UploadControl.PostedFile.InputStream);

                // resize de la imagen.
                if (curBitmap.Width > MaxSize || curBitmap.Height > MaxSize)
                    curBitmap = ToMaxSize(curBitmap, MaxSize, MaxSize);

                Width = curBitmap.Width;
                Height = curBitmap.Height;

                if (quality < 0 || quality > 100)
                    throw new ArgumentOutOfRangeException("quality debe estar entre 0 y 100.");


                // Encoder parameter for image quality
                EncoderParameter qualityParam =
                    new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                // Jpeg image codec
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;

                try
                {
                    curBitmap.Save(destino, jpegCodec, encoderParams);
                    curBitmap.Dispose();
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Retorna el tipo de codec de la imagen con el "mime type"
        /// </summary>
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Colocamos en un array todos los tipos de imágenes
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Buscamos el tipo de imagen correcto
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        //This will resize an image proportionally that will fit in the passing max size. 
        public static System.Drawing.Image ToMaxSize(System.Drawing.Image image, int maxWidth, int maxHeight)
        {
            return DoResize(image, ResizeType.PixelMax, maxWidth, maxHeight);
        }

        //This will resize an image exactly to the passing size
        public static System.Drawing.Image ToFixedSize(System.Drawing.Image image, int width, int height, FixedSizeOpt option)
        {
            System.Drawing.Image newImage = DoResize(image, ResizeType.PixelFixed, width, height);
            switch (option)
            {
                case FixedSizeOpt.FixedHeightCropWidth:
                    if (newImage.Width > width)
                        newImage = ToCroppedSize(image, width, image.Height, 0, 0);
                    break;
                case FixedSizeOpt.FixedWidthCropHeight:
                    if (newImage.Height > height)
                        newImage = ToCroppedSize(image, image.Width, height, 0, 0);
                    break;
            }
            return newImage;
        }

        //This will resize an image proportionally by the percentage
        public static System.Drawing.Image ToPercent(System.Drawing.Image image, double percent)
        {
            double p = percent / 100.0;
            int w = Convert.ToInt32(Math.Round((image.Width * p), 0));
            int h = Convert.ToInt32(Math.Round((image.Height * p), 0));
            return DoResize(image, ResizeType.Percent, w, h);
        }

        //This will crop an image to specified size
        public static System.Drawing.Image ToCroppedSize(System.Drawing.Image image, int targetWidth, int targetHeight, int targetX, int targetY)
        {
            int currentWidth = image.Width;
            int currentHeight = image.Height;

            if ((currentWidth <= targetWidth) && (currentHeight <= targetHeight)) { return image; }

            Bitmap newImage = new Bitmap(targetWidth, targetHeight);
            newImage.SetResolution(72, 72);
            Graphics gr = Graphics.FromImage(newImage);

            //just in case it's a transparent GIF force the bg to white 
            SolidBrush sb = new SolidBrush(System.Drawing.Color.White);
            gr.FillRectangle(sb, 0, 0, newImage.Width, newImage.Height);

            //gr.SmoothingMode = SmoothingMode.AntiAlias;
            //gr.InterpolationMode = InterpolationMode.Bicubic;
            //gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gr.DrawImage(image, new Rectangle(0, 0, targetWidth, targetHeight), targetX, targetY, targetWidth, targetHeight, GraphicsUnit.Pixel);

            return newImage;
        }

        private static System.Drawing.Image DoResize(System.Drawing.Image image, ResizeType type, int targetWidth, int targetHeight)
        {
            int newWidth = 0;
            int newHeight = 0;
            int currentWidth = image.Width;
            int currentHeight = image.Height;

            switch (type)
            {
                case ResizeType.Percent:
                    newWidth = targetWidth;
                    newHeight = targetHeight;
                    break;
                case ResizeType.PixelFixed:
                    newWidth = targetWidth;
                    newHeight = targetHeight;
                    break;
                case ResizeType.PixelMax:
                    if ((currentWidth / (double)targetWidth) > (currentHeight / (double)targetHeight))
                    {
                        newWidth = targetWidth;
                        newHeight = Convert.ToInt32(currentHeight * (targetWidth / (double)currentWidth));
                        if (newHeight > targetHeight)
                        {
                            newWidth = Convert.ToInt32(targetWidth * (targetHeight / (double)newHeight));
                            newHeight = targetHeight;
                        }
                    }
                    else
                    {
                        newWidth = Convert.ToInt32(currentWidth * (targetHeight / (double)currentHeight));
                        newHeight = targetHeight;
                        if (newWidth > targetWidth)
                        {
                            newWidth = targetWidth;
                            newHeight = Convert.ToInt32(targetHeight * (targetWidth / (double)newWidth));
                        }
                    }
                    break;
            }

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            newImage.SetResolution(72, 72); //web resolution;

            //create a graphics object 
            Graphics gr = Graphics.FromImage(newImage);

            //just in case it's a transparent GIF force the bg to white 
            SolidBrush sb = new SolidBrush(System.Drawing.Color.White);
            gr.FillRectangle(sb, 0, 0, newImage.Width, newImage.Height);

            //Re-draw the image to the specified height and width
            gr.DrawImage(image, 0, 0, newImage.Width, newImage.Height);

            return newImage;

        }

        public static void SaveJpegWithCompression(HttpPostedFileBase PostedFile, string Destino, int MaxSize, out int Width, out int Height)
        {
            if (PostedFile.ContentLength > 5242880)
                throw new ArgumentOutOfRangeException("La imagen es mayor a 5 MB: " + PostedFile.FileName);
            else
            {
                System.Drawing.Image curBitmap = Bitmap.FromStream(PostedFile.InputStream);

                // resize de la imagen.
                if (curBitmap.Width > MaxSize || curBitmap.Height > MaxSize)
                    curBitmap = ToMaxSize(curBitmap, MaxSize, MaxSize);

                Width = curBitmap.Width;
                Height = curBitmap.Height;

                // Encoder parameter for image quality
                EncoderParameter qualityParam =
                    new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);

                // Jpeg image codec
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;

                try
                {                    
                    curBitmap.Save(Destino, jpegCodec, encoderParams);
                    curBitmap.Dispose();
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }

    }
}