// Copyright © 2010-2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Log;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ITCSScraper
{
    public class UploadFileDialogHandler : IDialogHandler
    {

        private string _currentFilePath;
        public string CurrentFilePath
        {
            get { return _currentFilePath; }
            set { _currentFilePath = value; }
        }

        public UploadFileDialogHandler(string filePath)
        {
            CurrentFilePath = filePath;
        }

        public bool OnFileDialog(IWebBrowser browserControl, IBrowser browser, CefFileDialogMode mode, string title, string defaultFilePath, List<string> acceptFilters, int selectedAcceptFilter, IFileDialogCallback callback)
        {
            // Leer esto desde la bd
            //string filePath = @"C:/Users/arojas/Desktop/New folder/20180115 Instapago Cargo en Cuenta 2018011502.txt";

            string filePath = CurrentFilePath;

            //string filePath = CurrentChargeAccount.Ruta;

            callback.Continue(selectedAcceptFilter, new List<string> { /*Path.GetRandomFileName()*/ filePath });

            // Presionamos "Enviar"
            browser.MainFrame.ExecuteJavaScriptAsync("document.getElementById('ctl00_cp_wz_EnvioEE_StartNavigationTemplateContainerID_btnAceptar').click()");

            Logger.WriteSuccessLog("Cargo el archivo", "UploadFileDialogHandler (OnFileDialog)");

            return true;
        }
    }
}
