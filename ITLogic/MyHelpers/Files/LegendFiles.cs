using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;

public class LegendFiles
{
    #region Vars
    #region Private
    private String[] AllowExtension = { ".jpg", ".jpeg", ".bmp", ".gif", ".png" };

    private Size thumbNailSize;
    private HttpServerUtility Server;

    private String absolutePath;//C:/Tuticket/WebSite/
    private String virtualPath;//http://www.tuticket.com/
    private String uriDirectory;//~/Public/Images/Evento/10/
    private String pathDirectory;//C:/Tuticket/Tuticket2.0/Public/Images/Evento/10/
    private String uriFile;//~/Public/Images/Evento/10/Archivo.jpg
    private String pathFile;//C:/Tuticket/Tuticket2.0/Public/Images/Evento/10/Archivo.jpg

    #endregion
    #region Properties
    /// <summary>
    /// ~/Public/Images/Evento/10/Archivo.jpg
    /// </summary>
    public String UriFile { get { return uriFile; } }
    public Size ThumbNailSize { get { return thumbNailSize; } set { thumbNailSize = value; } }
    /// <summary>
    /// http://www.tuticket.com/Public/Images/Banners/BannerEvento/10/Archivo.jpg
    /// </summary>
    /// 
    public String UriFileExtension
    {
        get
        {
            return Path.GetExtension(pathFile);
        }
    }
    public String VirtualFile
    {
        get
        {
            try
            {
                return virtualPath + uriFile.Remove(0, 2);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
    #endregion
    #endregion
    #region Constructors
    public LegendFiles(String UriDirectory)
    {
        this.absolutePath = System.Configuration.ConfigurationManager.AppSettings["AbsolutePath"];
        this.virtualPath = System.Configuration.ConfigurationManager.AppSettings["VirtualPath"];
        Init(UriDirectory);
    }
    public LegendFiles(String UriDirectory, HttpServerUtility Server)
    {
        this.Server = Server;
        Init(UriDirectory);
    }
    private void Init(String UriDirectory)
    {
        uriFile = null;
        pathFile = null;
        this.uriDirectory = UriDirectory;
        if (Server != null)
            this.pathDirectory = Server.MapPath(UriDirectory);
        else
            this.pathDirectory = absolutePath + uriDirectory.Remove(0, 2).Replace("/", "\\");
        DirectoryInfo dir = new DirectoryInfo(pathDirectory);
        try
        {
            FileInfo[] files = dir.GetFiles();
            if (files.Length > 0)
            {
                uriFile = uriDirectory + "/" + files[0].Name;
                pathFile = pathDirectory + "\\" + files[0].Name;
            }
        }
        catch (DirectoryNotFoundException)
        {
            uriFile = null;
        }
    }
    #endregion

    #region Publics Methods
    public void UploadElement(FileUpload FileUploadElement)
    {
        Directory.CreateDirectory(pathDirectory);
        DeleteFilesForFolder();
        FileUploadElement.PostedFile.SaveAs(pathDirectory + "/" + FileUploadElement.FileName);
        uriFile = uriDirectory + "/" + FileUploadElement.FileName;
        pathFile = pathDirectory + "/" + FileUploadElement.FileName;
    }
    public void UploadImage(FileUpload FileUploadImage)
    {
        if (FileUploadImage.HasFile)
        {
            Directory.CreateDirectory(pathDirectory);
            Boolean OkFormat = false;
            String extension = Path.GetExtension(FileUploadImage.FileName).ToLower();
            for (int i = 0; i < AllowExtension.Length; i++)
            {
                if (extension == AllowExtension[i])
                    OkFormat = true;
            }
            if (OkFormat)
            {
                DeleteFilesForFolder();
                FileUploadImage.PostedFile.SaveAs(pathDirectory + "/" + FileUploadImage.FileName);
                uriFile = uriDirectory + "/" + FileUploadImage.FileName;
                pathFile = pathDirectory + "/" + FileUploadImage.FileName;
                if (ThumbNailSize != System.Drawing.Size.Empty)
                    GenerateThumbNailImagesForFolder(pathDirectory);
            }
        }
    }
    public void UploadImage(FileUpload FileUploadImage, Size ThumbNailSize)
    {
        this.ThumbNailSize = ThumbNailSize;
        UploadImage(FileUploadImage, ThumbNailSize);
    }
    public void GenerateThumbNail(String UriDirectoryTarget, Size ThumbNailSize)
    {
        this.ThumbNailSize = ThumbNailSize;
        // Creating PathDirectoryTarget
        String PathDirectoryTarget = absolutePath + UriDirectoryTarget.Remove(0, 2).Replace("/", "\\");
        Boolean SameDirectory = false;
        if (UriDirectoryTarget != uriDirectory)
        {
            Directory.CreateDirectory(PathDirectoryTarget);
            DeleteFilesForFolder(PathDirectoryTarget);
        }
        else
            SameDirectory = true;
        //Gathering Original Files
        DirectoryInfo oDir = new DirectoryInfo(pathDirectory);
        FileInfo[] oFiles = oDir.GetFiles();
        foreach (FileInfo oFile in oFiles)
        {
            String UriFileTarget = Path.GetFileNameWithoutExtension(oFile.Name) + "_thumbnails" + Path.GetExtension(oFile.Name);
            oFile.CopyTo(PathDirectoryTarget + UriFileTarget, true);
            if (SameDirectory)
                oFile.Delete();
            uriFile = UriDirectoryTarget + UriFileTarget;
            pathFile = UriDirectoryTarget + UriFileTarget;
        }
        GenerateThumbNailImagesForFolder(PathDirectoryTarget);
    }
    public void DeleteFolder()
    {
        try
        {
            DirectoryInfo oDir = new DirectoryInfo(pathDirectory);
            oDir.Delete(true);
        }
        catch (DirectoryNotFoundException) { }
    }
    public void ChangeUriDirectory(String UriDirectory)
    {
        Init(UriDirectory);
    }
    #endregion

    #region Private Methods
    private void DeleteFilesForFolder()
    {
        DeleteFilesForFolder(pathDirectory);
    }
    private void DeleteFilesForFolder(String PathDirectory)
    {
        try
        {
            DirectoryInfo oDir = new DirectoryInfo(PathDirectory);
            FileInfo[] oFil = oDir.GetFiles();
            for (int i = 0; i < oFil.Length; i++)
            {
                oFil[i].Delete();
            }
        }
        catch (DirectoryNotFoundException) { }
    }
    private void GenerateThumbNailImagesForFolder(string FolderName)
    {
        string sPhysicalPath = FolderName;
        string sFileName = "";

        DirectoryInfo oDir = new DirectoryInfo(sPhysicalPath);

        try
        {

            //   FileInfo[] oDeleteFiles = oDir.GetFiles();

            FileInfo[] oFiles = oDir.GetFiles();

            foreach (FileInfo oFile in oFiles)
            {

                sFileName = oFile.Name.ToLower();

                if (sFileName.IndexOf(".gif") > 0)
                {
                    this.GenerateThumbNail(sPhysicalPath, sFileName, sFileName, ImageFormat.Gif);
                }
                if (sFileName.IndexOf(".jpg") > 0)
                {
                    this.GenerateThumbNail(sPhysicalPath, sFileName, sFileName, ImageFormat.Jpeg);
                }
                if (sFileName.IndexOf(".bmp") > 0)
                {
                    this.GenerateThumbNail(sPhysicalPath, sFileName, sFileName, ImageFormat.Bmp);
                }
                if (sFileName.IndexOf(".png") > 0)
                {
                    this.GenerateThumbNail(sPhysicalPath, sFileName, sFileName, ImageFormat.Png);
                }
            }
        }
        catch (Exception) { }
    }
    private void GenerateThumbNail(string sPhysicalPath, string sOrgFileName, string sThumbNailFileName, ImageFormat oFormat)
    {
        try
        {
            System.Drawing.Image oImg = System.Drawing.Image.FromFile(sPhysicalPath + @"\" + sOrgFileName);
            ThumbNailSize = NewSize(new Size(oImg.Width, oImg.Height));
            System.Drawing.Image oThumbNail = new Bitmap(this.ThumbNailSize.Width, this.ThumbNailSize.Height, oImg.PixelFormat);
            Graphics oGraphic = Graphics.FromImage(oThumbNail);
            oGraphic.CompositingQuality = CompositingQuality.HighQuality;
            oGraphic.SmoothingMode = SmoothingMode.HighQuality;
            oGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            Rectangle oRectangle = new Rectangle(0, 0, this.ThumbNailSize.Width, this.ThumbNailSize.Height);
            oGraphic.DrawImage(oImg, oRectangle);
            oImg.Dispose();
            oThumbNail.Save(sPhysicalPath + @"\" + sThumbNailFileName, oFormat);
        }
        catch (Exception) { }
    }
    private Size NewSize(Size img)
    {
        if (img.Width > ThumbNailSize.Width)
        {
            float dif = Diference(ThumbNailSize.Width, img.Width);
            img.Width = Convert.ToInt32(img.Width * dif);
            img.Height = Convert.ToInt32(img.Height * dif);
        }
        if (img.Height > ThumbNailSize.Height)
        {
            float dif = Diference(ThumbNailSize.Height, img.Height);
            img.Width = Convert.ToInt32(img.Width * dif);
            img.Height = Convert.ToInt32(img.Height * dif);
        }
        return img;
    }
    private float Diference(int O_x, int O_xP)
    {
        return ((float)O_x) / ((float)O_xP);
    }
    #endregion
    public Boolean EnableElements(System.Web.UI.WebControls.Image img, Boolean VirtualUrl)
    {
        try
        {
            if (!VirtualUrl)
            {
                if (UriFile != null)
                {
                    img.ImageUrl = UriFile;
                    img.Visible = true;
                    return true;
                }
                else { return false; }
            }
            else
            {
                if (VirtualFile != null)
                {
                    img.ImageUrl = VirtualFile;
                    img.Visible = true;
                    return true;
                }
                else { return false; }
            }
        }
        catch (Exception) { return false; }
    }
}
