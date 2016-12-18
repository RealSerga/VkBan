using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using VkNet;
using VkNet.Exception;
using VkNet.Model.RequestParams;
using VarUtility;
namespace VkBan
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
            PassTextBox.PasswordChar = '*';
        }
        VkApi vk = new VkApi();
        long captchssid;
        private async void OkButton_Click(object sender, EventArgs e)
        {
            miniprogressBar.Value = 0;
            progressBar.Value = 0;


            try
            {
                toolStripStatusLabel.Text = "Авторизация...";
                if (TwoAuthChekBox.Checked)
                {
                    TwoAuthTextBox.Visible = true;
                    TwoAuthTextBox.ReadOnly = true;
                    Func<string> code = () =>
                    {
                        
                        return TwoAuthTextBox.Text;
                    };
                    await vk.AuthorizeAsync(new ApiAuthParams
                    {
                        ApplicationId = 5783401,
                        Login = LoginTextBox.Text,
                        Password = PassTextBox.Text,
                        Settings = VkNet.Enums.Filters.Settings.All,
                        TwoFactorAuthorization = code
                    });
                }
                else
                {
                    await vk.AuthorizeAsync(new ApiAuthParams
                    {
                        ApplicationId = 5783401,
                        Login = LoginTextBox.Text,
                        Password = PassTextBox.Text,
                        Settings = VkNet.Enums.Filters.Settings.All
                    });
                }



                MessageBox.Show("Авторизация успешна", "Ok", MessageBoxButtons.OK, MessageBoxIcon.Question);
                toolStripStatusLabel.Text = "Авторизация успешна";
                KeyTextBox.ReadOnly = true;
                LoginTextBox.ReadOnly = true;
                PassTextBox.ReadOnly = true;
                backgroundWorker.RunWorkerAsync();
            }
            catch (CaptchaNeededException exe)
            {
                MessageBox.Show("Нужна капча", "Капча", MessageBoxButtons.OK, MessageBoxIcon.Information);
                toolStripStatusLabel.Text = "Капча";
                webBrowser.Visible = true;
                CatchTextBox.Visible = true;
                CaptchButton.Visible = true;
                webBrowser.Url = exe.Img;
                captchssid = exe.Sid;

            }
            catch(VkApiAuthorizationException es)
            {
                MessageBox.Show("Неверный логин и/или пароль", "Неверный логин и/или пароль", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void CaptchButton_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripStatusLabel.Text = "Авторизуйтесь";
                vk.Authorize(new ApiAuthParams
                {
                    ApplicationId = 5783401,
                    Login = LoginTextBox.Text,
                    Password = PassTextBox.Text,
                    CaptchaKey = CatchTextBox.Text,
                    CaptchaSid = captchssid
                });
            }
            catch (CaptchaNeededException exe)
            {
                MessageBox.Show("Нужна капча", "Капча", MessageBoxButtons.OK, MessageBoxIcon.Information);
                toolStripStatusLabel.Text = "Капча";
                webBrowser.Visible = true;
                CatchTextBox.Visible = true;
                CaptchButton.Visible = true;
                webBrowser.Url = exe.Img;
                captchssid = exe.Sid;

            }
            webBrowser.Visible = false;
            CatchTextBox.Visible = false;
            CaptchButton.Visible = false;
            OkButton_Click(sender, e);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            String[] keys = KeyTextBox.Text.Split(',');
            int j = 0;
            foreach (var key in keys)
            {
                j++;
                for (int i = 1; i < 3; i++)
                {

                    var groups = vk.Groups.Search(new GroupsSearchParams { Query = key, Offset = 100 * i - 100, Count = 100 * i });
                    List<long> banslist = new List<long>();
                    int d = 0;
                    foreach (var gro in groups)
                    {
                        d++;
                        banslist.Add(gro.Id);
                        File.AppendAllText("ids.txt", gro.Id + ";");
                        backgroundWorker.ReportProgress((int)VarUtility.Math.map(d, 0, groups.Count, 0, 100));

                    }

                    try
                    {
                        vk.NewsFeed.AddBan(new List<long>(), banslist);
                    }
                    catch
                    {

                    }
                    banslist.Clear();
                }

                backgroundWorker.ReportProgress((int)VarUtility.Math.map(j, 0, keys.LongLength, 0, 100) * 10000);






            }
            MessageBox.Show("Успешно!", "Успешно!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 10000)
            {
                if (e.ProgressPercentage <= 100 && 0 <= e.ProgressPercentage)

                {
                    miniprogressBar.Value = e.ProgressPercentage;
                    if (progressBar.Value < 1)
                    {
                        progressBar.Value = 1;
                    }
                }
            }
            else
            {
                if (e.ProgressPercentage / 10000 <= 100 && 0 <= e.ProgressPercentage / 10000)

                {
                    progressBar.Value = (e.ProgressPercentage / 10000);
                }

            }
            toolStripStatusLabel.Text = "Отправка";
        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }


        private void backgroundWorker_RunWorkerCompleted(object sendor, RunWorkerCompletedEventArgs e)
        {
            KeyTextBox.ReadOnly = false;
            KeyTextBox.ReadOnly = false;
            LoginTextBox.ReadOnly = false;
            PassTextBox.ReadOnly = false;

        }

        private void TwoAuth_CheckedChanged(object sender, EventArgs e)
        {
            if(!TwoAuthChekBox.Checked)
            {
                TwoAuthTextBox.Visible = false;
                label4.Visible = false;
            }
            else
            {
                TwoAuthTextBox.Visible = true;
                label4.Visible = true;
            }
        }

        private void TwoAuthTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            
        }
    }
}
