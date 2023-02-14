using GCBM.Properties;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace GCBM
{
    public partial class frmLanguagePrompt : Form
    {
        public IniFile ini = Program.ConfigFile;
        public frmLanguagePrompt()
        {
            InitializeComponent();
        }

        private void LanguagePrompt_Load(object sender, EventArgs e)
        {
            this.Text = Resources.LanguagePromptTitle;
            string sysLang = Program.DetectOSLanguage();

            foreach (var c in Program.CultureInfos)
            {
                cbSupportedCultures.Items.Add(c.NativeName + " [" + c.Name + "]");
            }
        }

        private void btnSetLanguage_Click(object sender, EventArgs e)
        {
            Program.ConfigFile.IniWriteString("LANGUAGE", "ConfigLanguage", Program.CultureInfos[cbSupportedCultures.SelectedIndex].Name);
            this.Dispose();
        }

        private void cbSupportedCultures_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Refresh the interface to give the user a preview of the selected language via updating the label text
            CultureInfo.CurrentUICulture = new CultureInfo(Program.CultureInfos[cbSupportedCultures.SelectedIndex].Name);
            MessageBox.Show(Resources.LanguagePromptConfirm, Resources.LanguagePromptConfirmTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Refresh();
        }
    }
}
