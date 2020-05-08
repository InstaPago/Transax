using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace MyHelpers.Files
{
    public class FileUploadHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {

                #region Read and Prep Variables

                int chunk = context.Request.QueryString["chunk"] != null ? int.Parse(context.Request.QueryString["chunk"]) : 0;
                int chunks = context.Request.QueryString["chunks"] != null ? int.Parse(context.Request.QueryString["chunks"]) : 0;
                string fileName = context.Request.QueryString["name"] != null ? context.Request.QueryString["name"] : "";
                string RepositoryPath = "Files/Andre"; //It is possible to pass this via the headers (default functionality of PLUpload)

                fileName = fileName.Replace("/", "").Replace("'", "").Replace("\\", "");

                string TargetPath = context.Server.MapPath(string.Format("~/{0}/", RepositoryPath));
                string ActualPathAndFile = Path.Combine(TargetPath, fileName);
                string TempPathAndFile = ActualPathAndFile + ".uploading";

                Byte[] buffer = null;

                #endregion

                #region Read the Posted File (or chunk of it)

                if (context.Request.ContentType == "application/octet-stream" && context.Request.ContentLength > 0)
                {
                    buffer = new Byte[context.Request.InputStream.Length];
                    context.Request.InputStream.Read(buffer, 0, buffer.Length);
                }
                else if (context.Request.ContentType.Contains("multipart/form-data") && context.Request.Files.Count > 0 && context.Request.Files[0].ContentLength > 0)
                {
                    buffer = new Byte[context.Request.Files[0].InputStream.Length];
                    context.Request.Files[0].InputStream.Read(buffer, 0, buffer.Length);
                    if (fileName == string.Empty) { fileName = context.Request.Files[0].FileName; }
                }

                #endregion

                if (buffer != null)
                {
                    try
                    {
                        #region Check if the target folder exists

                        //check if the path exist
                        string[] arrFolders = TargetPath.Split("\\".ToCharArray());
                        string TestForPath = "";
                        for (int f = 0; f < arrFolders.Length; f++)
                        {
                            if (arrFolders[f] != string.Empty)
                            {
                                TestForPath += arrFolders[f] + "\\";
                                if (!Directory.Exists(TestForPath))
                                {
                                    Directory.CreateDirectory(TestForPath);
                                }
                            }
                        }

                        #endregion

                        try
                        {

                            #region If this is the first chunk and the file exists, remove it

                            if (chunk == 0 && (File.Exists(TempPathAndFile) || File.Exists(ActualPathAndFile)))
                            {
                                File.Delete(TempPathAndFile);
                                File.Delete(ActualPathAndFile);
                            }
                            #endregion

                            #region Write the chunk of bytes to the file

                            using (FileStream fs = new FileStream(TempPathAndFile, chunk == 0 ? FileMode.OpenOrCreate : FileMode.Append))
                            {
                                File.SetAttributes(TempPathAndFile, FileAttributes.Hidden);
                                fs.Write(buffer, 0, buffer.Length);
                                fs.Close();
                            }

                            #endregion

                            #region Make the file available on the server

                            if (chunk == chunks - 1)
                            {
                                File.Move(TempPathAndFile, ActualPathAndFile); // remove the '.uploading';
                                File.SetAttributes(ActualPathAndFile, FileAttributes.Normal);
                            }

                            #endregion

                            context.Response.Write("{'jsonrpc' : '2.0', 'result' : null, 'id' : 'id'}");
                        }
                        catch (Exception)
                        {
                            context.Response.Write("{'jsonrpc' : '2.0', 'error' : {'code': 103, 'message':'Insufficient permissions to save the file'}, 'id' : 'id'}");

                        }

                    }
                    catch (Exception)
                    {
                        context.Response.Write("{'jsonrpc' : '2.0', 'error' : {'code': 102, 'message':'Insufficient permissions to create the target folder'}, 'id' : 'id'}");
                    }
                }


            }
            catch (Exception ex)
            {
                context.Response.Write("{'jsonrpc' : '2.0', 'error' : {'code': 101, 'message':'" + ex.Message + "'}, 'id' : 'id'}");

            }
        }
    }
}
