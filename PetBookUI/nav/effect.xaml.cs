using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using PetBook.core;
using System.Runtime.InteropServices;
using PetBook.typing;
using System.Threading;

namespace PetBook.nav
{
    unsafe public partial class EffectPage : Page
    {
        public EffectPage()
        {
            InitializeComponent();
            this.mode.ItemsSource = new string[] { "搜索技能效果ID", "搜索魂印ID" };
            this.mode.SelectedIndex = 0;
            this.listView.SelectionChanged += listView_SelectionChanged;
            this.listView.PreviewMouseDown += listView_SelectionChanged;
        }
        private void Dialog_PrimaryButtonClick(ModernWpf.Controls.ContentDialog sender, ModernWpf.Controls.ContentDialogButtonClickEventArgs args)
        {
            this.dialog.Hide();
        }
        unsafe private void button_Click(object sender, RoutedEventArgs e)
        {
            string x = this.gui_search_str.Text;
            int num;
            try
            {
                num = Int32.Parse(x);

            }
            catch (Exception)
            {
                this.detail.Dispatcher.Invoke(delegate
                {
                    this.detail.Children.Clear();
                    this.detail.Children.Add(new TextBlock
                    {
                        Text = "请输入ID",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 15,
                        Width = 260
                    });
                });
                this.listView.ItemsSource = new List<IntStringPair>();
                return;
            }
            if (this.mode.SelectedIndex == 0)
            {
                new Thread(new ThreadStart(delegate
                {
                    Thread t1 = new Thread(new ThreadStart(delegate
                    {
                        CBaseDetail* res = (CBaseDetail*)FFI.query_skill_effect(num);

                        if (res == null)
                        {
                            this.detail.Dispatcher.Invoke(delegate
                            {
                                this.detail.Children.Clear();
                                this.detail.Children.Add(new TextBlock
                                {
                                    Text = "no result",
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 15,
                                    Width = 260
                                });
                            });
                        }
                        else
                        {
                            int cnt = (int)res->id;
                            string desc = Marshal.PtrToStringUTF8((IntPtr)res->name)!;
                            FFI.free_skill_effect((void*)res);
                            StringBuilder sb = new StringBuilder();
                            sb.Append("ID：");
                            sb.AppendLine(x);
                            sb.Append("描述：");
                            sb.AppendLine(desc);
                            sb.Append("参数量：");
                            sb.AppendLine(cnt.ToString());
                            sb.AppendLine("(点击右侧查看详情)");
                            this.detail.Dispatcher.Invoke(delegate
                            {
                                this.detail.Children.Clear();
                                this.detail.Children.Add(new TextBlock
                                {
                                    Text = sb.ToString(),
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 15,
                                    Width = 260
                                });
                            });
                        }
                    }));
                    Thread t2 = new Thread(new ThreadStart(delegate
                    {
                        CArray* res = (CArray*)FFI.query_skill_effect_related(num);
                        CBaseDetail* cur = (CBaseDetail*)res->ptr;
                        List<IntStringPair> vcc = new List<IntStringPair>();
                        for (int i = 0; i < res->len; i += 1)
                        {
                            int id = (int)cur->id;
                            string str = Marshal.PtrToStringUTF8((IntPtr)cur->name)!;
                            vcc.Add(new IntStringPair(id, str));
                            cur += 1;
                        }
                        FFI.free_int_string_pair_list((void*)res);
                        this.listView.Dispatcher.Invoke(delegate
                        {
                            this.listView.ItemsSource = vcc;
                        });
                    }));

                    t1.Start();
                    t2.Start();
                    t1.Join();
                    t2.Join();
                })).Start();
            }
            else
            {
                new Thread(new ThreadStart(delegate
                {
                    Thread t1 = new Thread(new ThreadStart(delegate

                    {
                        CBuff* cur = (CBuff*)FFI.query_buff(num);
                        if (cur == null)
                        {
                            this.detail.Dispatcher.Invoke(delegate
                            {
                                this.detail.Children.Clear();
                                this.detail.Children.Add(new TextBlock
                                {
                                    Text = "no result",
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 15,
                                    Width = 260
                                });
                            });
                        }
                        else
                        {
                            Int32 id = cur->id;
                            Int32 effect = cur->effect_id;
                            string tips = Marshal.PtrToStringUTF8((IntPtr)cur->tips)!;
                            string url = Marshal.PtrToStringUTF8((IntPtr)cur->url)!;
                            string come = Marshal.PtrToStringUTF8((IntPtr)cur->come)!;

                            string s1 = String.Format("ID {0}", id);
                            string s2 = String.Format("效果ID {0}", effect);
                            var split = tips.Split("|");
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            foreach (string s in split)
                            {
                                sb.AppendLine(s);
                            }
                            string s3 = sb.ToString();
                            sb.Clear();
                            sb.AppendLine(s1);
                            sb.AppendLine(s2);
                            sb.AppendLine(s3);
                            if (url != "null")
                            {
                                sb.AppendLine(url);
                            }
                            if (come != "null")
                            {
                                sb.AppendLine(come);
                            }
                            this.detail.Dispatcher.Invoke(delegate
                            {
                                this.detail.Children.Clear();
                                this.detail.Children.Add(new TextBlock
                                {
                                    Text = sb.ToString(),
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 15,
                                    Width = 260
                                });
                            });
                            FFI.free_buff((void*)cur);
                        }
                    }));

                    Thread t2 = new Thread(new ThreadStart(delegate
                    {
                        CArray* res = (CArray*)FFI.query_buff_related(num);
                        CBaseDetail* cur = (CBaseDetail*)res->ptr;
                        List<IntStringPair> vcc = new List<IntStringPair>();
                        for (int i = 0; i < res->len; i += 1)
                        {
                            int id = (int)cur->id;
                            string str = Marshal.PtrToStringUTF8((IntPtr)cur->name)!;
                            vcc.Add(new IntStringPair(id, str));
                            cur += 1;
                        }
                        FFI.free_int_string_pair_list((void*)res);
                        this.listView.Dispatcher.Invoke(delegate
                        {
                            this.listView.ItemsSource = vcc;
                        });
                    }));

                    t1.Start();
                    t2.Start();
                    t1.Join();
                    t2.Join();
                })).Start();
            }
        }

        private void listView_SelectionChanged(object sender, EventArgs e)
        {

            if (this.listView.SelectedItem == null) return;

            int selected = ((IntStringPair)this.listView.SelectedItem).i;
            if (this.mode.SelectedIndex == 0)
            {

                new Thread(new ThreadStart(delegate
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("拥有这个技能的有:");
                    Thread t1 = new Thread(new ThreadStart(delegate
                    {
                        CArray* x = (CArray*)FFI.query_skill_related(selected);
                        CBaseDetail* cur = (CBaseDetail*)x->ptr;
                        for (int i = 0; i < x->len; i += 1)
                        {
                            string parse = Marshal.PtrToStringUTF8((IntPtr)cur->name)!;
                            sb.Append(parse);
                            if (i + 1 != x->len)
                            {
                                sb.Append(',');
                            }
                            cur += 1;
                        }
                        FFI.free_int_string_pair_list((void*)x);
                    }));
                    t1.Start();
                    CSkill* skill = (CSkill*)FFI.query_skill_detail(selected);
                    Skill sk = new Skill(skill);
                    FFI.free_single_skill((void*)skill);
                    t1.Join();

                    this.dialog_panel.Dispatcher.Invoke(() =>
                    {

                        this.dialog_panel.Children.Clear();
                        this.dialog_panel.Children.Clear();
                        this.dialog_panel.Children.Add(new SkillControlWide(sk));
                        this.dialog_panel.Children.Add(new TextBlock
                        {
                            Text = sb.ToString(),
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 16,
                            Margin = new Thickness(5, 5, 5, 5),
                        });
                        this.dialog.ShowAsync(ModernWpf.Controls.ContentDialogPlacement.Popup);

                    });
                })).Start();
            }
            else
            {
                List<string> vcc = PetBook.core.utils.get_buff(selected);

                this.dialog_panel.Dispatcher.Invoke(() =>
                {
                    this.dialog_panel.Children.Clear();
                    foreach (string i in vcc)
                    {
                        TextBlock block = new TextBlock()
                        {
                            Text = i,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 15,
                            Width = 260
                        };
                        this.dialog_panel.Children.Add(block);
                    }
                    this.dialog.ShowAsync(ModernWpf.Controls.ContentDialogPlacement.Popup);
                });
            }
        }
    }
}
