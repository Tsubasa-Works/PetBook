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
    public partial class MintMarkPage : Page
    {
        public MintMarkPage()
        {
            InitializeComponent();
            this.order_box.ItemsSource = new string[] { "ID", "ID逆序", "体力", "攻击", "防御", "特攻", "特防", "速度", "总和" };
            this.order_box.SelectedIndex = 0;
            this.gui_search_str.TextChanged += on_search;
            update_list();
        }

        unsafe private void update_list()
        {
            int mode = this.order_box.SelectedIndex;
            void* res;
            if (this.gui_search_str.Text.Length == 0)
            {
                res = FFI.query_mintmark_list(1 << mode);
            }
            else
            {
                fixed (byte* s = System.Text.Encoding.UTF8.GetBytes(this.gui_search_str.Text))
                {
                    res = FFI.query_mintmark_list_with_condition((void*)s, 1 << mode);
                }
            }
            List<MintMark> vPet = new List<MintMark>();
            CArray* x = (CArray*)res;
            CMintMark* cur = (CMintMark*)x->ptr;
            for (int i = 0; i < x->len; i += 1)
            {
                vPet.Add(new MintMark(cur));
                cur += 1;
            }
            this.gui_pet_list.ItemsSource = vPet;
            FFI.free_mintmark_list((void*)x);
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
