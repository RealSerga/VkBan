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

namespace VkBan
{
    public partial class MainForm : Form
    {
        
        public MainForm()
        {
            InitializeComponent();
            
        }
        VkApi vk = new VkApi();
        long captchssid;
        private void OkButton_Click(object sender, EventArgs e)
        {
            


            try
            {
                
                vk.Authorize(new ApiAuthParams
                {
                    ApplicationId = 5783401,
                    Login = LoginTextBox.Text,
                    Password = PassTextBox.Text,
                    Settings = VkNet.Enums.Filters.Settings.All
                });
               
                
                MessageBox.Show("Авторизация успешна","Ok",MessageBoxButtons.OK,MessageBoxIcon.Question);
                toolStripStatusLabel.Text = "Авторизация успешна";
                KeyTextBox.ReadOnly = true;
                backgroundWorker.RunWorkerAsync();
            }catch (CaptchaNeededException exe)
            {
                MessageBox.Show("Нужна капча", "Капча", MessageBoxButtons.OK, MessageBoxIcon.Information);
                toolStripStatusLabel.Text = "Капча";
                webBrowser.Visible = true;
                CatchTextBox.Visible = true;
                CaptchButton.Visible = true;
                webBrowser.Url = exe.Img;
                captchssid = exe.Sid;

            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
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
            }catch (CaptchaNeededException exe)
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
                    int totalCount;
                    var groups = vk.Groups.Search(out totalCount, new GroupsSearchParams { Query = key, Offset = 100 * i - 100, Count = 100 * i });
                    List<long> banslist = new List<long>();
                    int d = 0;
                    foreach (var gro in groups)
                    {
                        d++;
                        banslist.Add(gro.Id);
                        File.AppendAllText("ids.txt", gro.Id + ";");
                        backgroundWorker.ReportProgress((100/groups.Count) * d);

                    }

                    try
                    {
                        vk.NewsFeed.AddBan(new List<long>(), banslist);
                    }
                    catch {
                        
                    }
                    banslist.Clear();
                }

                backgroundWorker.ReportProgress(((100 / keys.Length) * j) * 10000);

                




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
                    if (progressBar.Value<1)
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

        
    }
}
