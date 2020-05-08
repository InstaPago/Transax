using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Security;
using System;
using System.Windows.Forms;


namespace InstaTransfer.RegistratorView
{
    /// <summary>
    /// Ventana de seleccion del rango de fechas
    /// </summary>
    public partial class URegistrator : Form
    {
        //Todo(Registrator): Pasar a MVP

        #region Variables

        private UUser newUser;
        private Bank bank;

        #endregion

        #region Presenter

        //private SBankSelectionPresenter _presenter;

        #endregion

        #region Model

        //public Button BBanesconline
        //{
        //    get { return bBanesconline; }

        //    set { bBanesconline = value; }
        //}

        //public Button BProvinet
        //{
        //    get { return bProvinet; }

        //    set { bProvinet = value; }
        //}

        #endregion

        #region Constructor

        public URegistrator()
        {
            InitializeComponent();

            //_presenter = new SBankSelectionPresenter(this);


            //if (args.Count() > 0)
            //{
            //    var scraper = ScraperFactory.GetScraper(GeneralHelper.GetBankEnum(args[0]));
            //    scraper.Show();
            //}
            //Logica de inicio de scrapers
        }

        #endregion

        private void bBanesconline_Click(object sender, EventArgs e)
        {
            bank = Bank.Banesco;
            bBanesconline.BackColor = System.Drawing.Color.LightGray;
            bProvinet.BackColor = System.Drawing.Color.White;
        }

        private void bProvinet_Click(object sender, EventArgs e)
        {
            bank = Bank.Provincial;
            bProvinet.BackColor = System.Drawing.Color.LightGray;
            bBanesconline.BackColor = System.Drawing.Color.White;
        }

        private void bSubmit_Click(object sender, EventArgs e)
        {
            newUser = new UUser();

            try
            {

                if (bank == 0 || 
                    string.IsNullOrWhiteSpace(tbUsername.Text) || 
                    string.IsNullOrWhiteSpace(tbPassword.Text) || 
                    string.IsNullOrWhiteSpace(tbSocialReason.Text))
                {
                    throw new Exception();
                }

                newUser.Username = ITSecurity.EncryptUserCredentials(tbUsername.Text);
                newUser.Password = ITSecurity.EncryptUserCredentials(tbPassword.Text);
                newUser.IdUSocialReason = tbSocialReason.Text;
                newUser.StatusChangeDate = DateTime.Now;
                newUser.IdUserStatus = 1;
                newUser.IdUBank = GeneralHelper.GetBankIdString(bank);



                URepository<UUser> UUserRepo = new URepository<UUser>();
                UUserRepo.AddEntity(newUser);
                UUserRepo.SaveChanges();

                lResult.Text = "Registro Exitoso";
                lResult.ForeColor = System.Drawing.Color.Green;
                DisableControls();


            }
            catch (Exception)
            {                
                lResult.Text = "Registro Fallido";
                lResult.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void lNewUser_Click(object sender, EventArgs e)
        {
            EnableControls();
        }


        void DisableControls()
        {
            bProvinet.Enabled = false;
            bBanesconline.Enabled = false;
            lNewUser.Visible = true;
            bSubmit.Enabled = false;
            tbPassword.Enabled = false;
            tbUsername.Enabled = false;
            tbSocialReason.Enabled = false;
            bCancel.ForeColor = System.Drawing.Color.Orange;
            bCancel.Text = "Salir";

        }

        void EnableControls()
        {

            bProvinet.Enabled = true;
            bBanesconline.Enabled = true;
            lNewUser.Visible = false;
            bSubmit.Enabled = true;
            tbPassword.Enabled = true;
            tbUsername.Enabled = true;
            tbSocialReason.Enabled = true;
            bCancel.Text = "Cancelar";
            bCancel.ForeColor = System.Drawing.Color.Black;

        }
    }
}
