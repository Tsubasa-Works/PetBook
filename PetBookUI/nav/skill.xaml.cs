using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class SkillPage : Page
    {
        int selected;
        public SkillPage()
        {
            InitializeComponent();
            this.gui_search_str.TextChanged += on_search;
            this.gui_search_result_list.SelectionChanged += on_select;
            update_list();
        }
        unsafe void update_list()
        {
            void* res;
            if (this.gui_search_str.Text.Length == 0)
            {
                res = FFI.query_skill_list();
            }
            else
            {
                fixed (byte* s = System.Text.Encoding.UTF8.GetBytes(this.gui_search_str.Text))
                {
                    res = FFI.query_skill_list_with_condition((void*)s);
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
            this.gui_search_result_list.ItemsSource = vPet;
            FFI.free_int_string_pair_list((void*)x);
        }
        unsafe void on_search(Object o, TextChangedEventArgs args)
        {
            update_list();
        }

        unsafe private void on_select(object o, SelectionChangedEventArgs e)
        {
            if (this.gui_search_result_list.SelectedItem == null) return;
            selected = ((IntStringPair)this.gui_search_result_list.SelectedItem).i;

            if (selected != 0)
            {
                new Thread(new ThreadStart(() =>
                {
                    Thread t1 = new Thread(new ThreadStart(() =>
                    {
                        CSkill* skill = (CSkill*)FFI.query_skill_detail(selected);
                        Skill sk = new Skill(skill);
                        this.detail.Dispatcher.Invoke(() =>
                        {
                            this.detail.Children.Clear();
                            this.detail.Children.Add(new SkillControlWide(sk));
                        });
                        FFI.free_single_skill((void*)skill);
                    }));

                    Thread t2 = new Thread(new ThreadStart(() =>
                    {
                        void* res = FFI.query_skill_related(selected);
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
                        FFI.free_int_string_pair_list((void*)x);
                        this.related_list.Dispatcher.Invoke(delegate
                        {
                            this.related_list.ItemsSource = vPet;
                        });
                    }));

                    t1.Start();
                    t2.Start();

                    t1.Join();
                    t2.Join();
                })).Start();
            }
        }
    }
}
