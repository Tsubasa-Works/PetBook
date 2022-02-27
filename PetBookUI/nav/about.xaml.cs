using System;
using System.Windows.Controls;
using System.Threading;
using PetBook.core;
namespace PetBook.nav
{
    public partial class AboutPage : Page
    {
        Thread updating_thread = null!;
        public AboutPage()
        {
            InitializeComponent();
        }

        private void on_update(object sender, EventArgs e)
        {
            this.updating_thread = new Thread(new ThreadStart(() => FFI.update()));
            this.updating_thread.Start();
            new Thread(new ThreadStart(this.update_daemon)).Start();
        }
        private void update_daemon()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(200);
                if (this.updating_thread.IsAlive)
                {
                    Int32 res = FFI.status();
                    if (res == -3)
                    {

                        this.update_status.Dispatcher.Invoke(delegate
                        {
                            this.update_status.Text = "更新图标中";
                        });

                    }
                    else if (res == -2)
                    {
                        this.update_status.Dispatcher.Invoke(delegate
                        {
                            this.update_status.Text = "更新尚未开始";
                        });
                    }
                    else if (res == -1)
                    {
                        this.update_status.Dispatcher.Invoke(delegate
                        {
                            this.update_status.Text = "更新数据库中";
                        });
                    }
                    else if (res >= 0)
                    {
                        this.update_status.Dispatcher.Invoke(delegate
                        {
                            this.update_status.Text = String.Format("更新二进制文件，已完成{0}", res);
                        });
                    }
                    else
                    {
                        this.update_status.Dispatcher.Invoke(delegate
                        {
                            this.update_status.Text = "未知错误";
                        });
                    }
                }
                else
                {
                    this.update_status.Dispatcher.Invoke(delegate
                    {
                        this.update_status.Text = "更新结束";
                    });
                    break;
                }
            }
            MainWindow.update_mapping();
        }
    }
}
