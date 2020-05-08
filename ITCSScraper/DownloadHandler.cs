// Copyright © 2010-2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using BLL.Concrete;
using CefSharp;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Scraper;
using System;
using System.Configuration;
using System.Text;

namespace ITCSScraper
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        public UUserBLL UUBLL = new UUserBLL();

        private UUser _currentUser;
        public UUser CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    return new UUser();
                }
                else
                {
                    return _currentUser;
                }

            }
            set { _currentUser = value; }
        }

        public DownloadHandler(UUser user)
        {
            CurrentUser = user;
        }

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            var bankName = GeneralHelper.GetBankEnum(CurrentUser.IdUBank).ToString();



            var handler = OnBeforeDownloadFired;
            if (handler != null)
            {
                handler(this, downloadItem);
            }

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    // Construimos la ruta del archivo
                    var pathBuilder = new StringBuilder(ConfigurationManager.AppSettings["BasePath"] + @"\");
                    pathBuilder.Append(CurrentUser.IdUSocialReason + @"\");
                    pathBuilder.Append(bankName + @"\");
                    pathBuilder.Append(DateTime.Now.ToString(ScraperResources.DateFormat) + ".csv");
                    var downloadPath = pathBuilder.ToString();

                    callback.Continue(downloadPath /*downloadItem.SuggestedFileName.Replace("txt", "csv")*/, showDialog: false);
                    //browser.MainFrame.ExecuteJavaScriptAsync("document.getElementById('ctl00_btnSalir_lkButton').click()");
                    
                    // Devolvemos el estado a activo
                    UUBLL.ChangeUserStatus(CurrentUser, UmbrellaUserStatus.Active);
                    // Cerramos sesion
                    browser.MainFrame.LoadUrl("https://ve1.provinet.net/shvp_ve_web/atpn_es_web_jsp/desconectar.jsp");

                    //// Cambio el estado a completada
                    //BSRBLL.CompleteRequest(CurrentRequest.Id, downloadPath);

                    // Navego a las cuentas
                    /*  browser.MainFrame.LoadUrl("https://www.banesconline.com/mantis/WebSite/default.aspx");*/
                }
            }
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            var handler = OnDownloadUpdatedFired;
            if (handler != null)
            {
                handler(this, downloadItem);
            }
        }
    }
}
