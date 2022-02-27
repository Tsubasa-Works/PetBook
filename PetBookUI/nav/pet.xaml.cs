using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PetBook.core;
using PetBook.typing;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Windows.Media.Imaging;
namespace PetBook.nav
{
    public partial class PetPage : Page
    {
        int selected = 0;
        public PetPage()
        {
            InitializeComponent();
            this.order_box.ItemsSource = new string[] { "ID", "ID逆序", "体力", "攻击", "防御", "特攻", "特防", "速度", "总和" };
            this.order_box.SelectedIndex = 0;
            this.gui_search_str.TextChanged += on_search;
            this.gui_pet_list.SelectionChanged += on_select;
            update_list();
        }
        private String safe_wrap(UInt32 x)
        {
            return x != 0 ? String.Format("{0}", x) : "-";
        }

        unsafe private void on_select(object o, SelectionChangedEventArgs e)
        {
            if (this.gui_pet_list.SelectedItem == null) return;
            selected = ((IntStringPair)this.gui_pet_list.SelectedItem).i;


            if (selected != 0)
            {
                new Thread(new ThreadStart(() =>
                {
                    Thread t1 = new Thread(new ThreadStart(() =>
                    {
                        var icon = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, Path.Combine(MainWindow.blob_base, String.Format("{0}.png", this.selected)));
                        try
                        {
                            this.head.Dispatcher.Invoke(delegate
                            {
                                this.head.Source = new BitmapImage(new Uri(icon, UriKind.Absolute));
                            });


                        }
                        catch (Exception)
                        {
                            this.head.Dispatcher.Invoke(delegate
                            {
                                icon = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, Path.Combine(MainWindow.blob_base, "1.png"));
                                this.head.Source = new BitmapImage(new Uri(icon, UriKind.Absolute));
                            });
                        }
                    }));

                    Thread t2 = new Thread(new ThreadStart(() =>
                    {
                        CPetFullDetail* res = (CPetFullDetail*)FFI.query_pet_detail(this.selected);


                        if (res->hp + res->atk + res->def + res->spatk + res->spdef + res->spd == 0) return;
                        List<String> v = new List<String> {
                            "ID "+safe_wrap(res->bases.id),
                            "名称 "+ Marshal.PtrToStringUTF8((IntPtr)res->bases.name)!,
                            "属性 "+MainWindow.mapping[(int)res->types].Item1,
                            "性别 "+res->gender switch{
                                0 => "-",
                                1 => "雄",
                                _ => "雌"
                            },
                            "进化 "+safe_wrap(res->evolving_lv),
                            "总和 "+safe_wrap(res->hp+res->atk+res->def+res->spatk+res->spdef+res->spd),

                            "体力 "+safe_wrap(res->hp),
                            "攻击 "+safe_wrap(res->atk),
                            "防御 "+safe_wrap(res->def),
                            "特攻 "+safe_wrap(res->spatk),
                            "特防 "+safe_wrap(res->spdef),
                            "速度 "+safe_wrap(res->spd),
                        };
                        List<Stringx3> src = new List<Stringx3>();
                        for (int i = 0; i < 4; i += 1)
                        {
                            src.Add(new Stringx3(v[i * 3], v[i * 3 + 1], v[i * 3 + 2]));
                        }
                        FFI.free_pet_detail((void*)res);
                        this.gui_value_list.Dispatcher.Invoke(delegate
                        {
                            this.gui_value_list.ItemsSource = src;
                        });
                    }));
                    Thread t3 = new Thread(new ThreadStart(() =>
                    {
                        List<string> vcc = PetBook.core.utils.get_buff(this.selected);
                        this.gui_buff_list.Dispatcher.Invoke(delegate
                        {
                            this.gui_buff_list.Children.Clear();
                            foreach (string i in vcc)
                            {
                                TextBlock block = new TextBlock()
                                {
                                    Text = i,
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 15,
                                    Width = 480
                                };
                                this.gui_buff_list.Children.Add(block);
                            }
                        });
                    }));
                    Thread t4 = new Thread(new ThreadStart(() =>
                    {
                        CArray* x = (CArray*)FFI.query_pet_skill(this.selected);

                        CSkill* cur = (CSkill*)x->ptr;
                        List<Skill> vcc = new List<Skill>();

                        for (int i = 0; i < x->len; i += 1)
                        {

                            vcc.Add(new Skill(cur));
                            cur += 1;
                        }
                        FFI.free_pet_skill((void*)x);
                        this.gui_skill_list.Dispatcher.Invoke(delegate
                        {
                            this.gui_skill_list.Children.Clear();
                            foreach (Skill skill in vcc)
                            {
                                this.gui_skill_list.Children.Add(new SkillControlWide(skill));
                            }
                        });
                    }));


                    t1.Start();
                    t2.Start();
                    t3.Start();
                    t4.Start();

                    t1.Join();
                    t2.Join();
                    t3.Join();
                    t4.Join();
                })).Start();
            }

        }
        unsafe private void update_list()
        {
            int mode = this.order_box.SelectedIndex;
            void* res;
            if (this.gui_search_str.Text.Length == 0)
            {
                res = FFI.query_pet_list(1 << mode);
            }
            else
            {
                fixed (byte* s = System.Text.Encoding.UTF8.GetBytes(this.gui_search_str.Text))
                {
                    res = FFI.query_pet_list_with_condition((void*)s, 1 << mode);
                }
            }
            List<IntStringPair> vPet = new List<IntStringPair>();
            CArray* x = (CArray*)res;
            CBaseDetail* cur = (CBaseDetail*)x->ptr;
            for (int i = 0; i < x->len; i += 1)
            {
                int id = (int)cur->id;
                string parse = Marshal.PtrToStringUTF8((IntPtr)cur->name)!;
                vPet.Add(new IntStringPair(id, parse));

                cur += 1;
            }
            this.gui_pet_list.ItemsSource = vPet;
            FFI.free_int_string_pair_list((void*)x);
        }

        void on_search(Object o, TextChangedEventArgs args)
        {
            update_list();
        }

        private void order_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            update_list();
        }
    }
}
