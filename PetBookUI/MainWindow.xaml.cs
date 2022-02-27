using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PetBook.nav;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System.Runtime.InteropServices;
using PetBook.core;

namespace PetBook
{
    public partial class MainWindow : Window
    {
        public static Dictionary<int, (string, bool)> mapping = new Dictionary<int, (string, bool)>();
        public const string blob_base = "./blob";

        private readonly List<(string Tag, Type PageType)> pages = new List<(string Tag, Type PageType)>()
        {
            ("pet", typeof(PetPage)),
            ("skill", typeof(SkillPage)),
            ("effect", typeof(EffectPage)),
            ("mintmark", typeof(MintMarkPage)),
            ("about", typeof(AboutPage)),
        };

        public MainWindow()
        {
            FFI.create_model();
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[0];
            update_mapping();
        }
        static unsafe public void update_mapping()
        {
            mapping.Clear();
            CArray* x = (CArray*)FFI.query_skill_type();
            CSkillType* cur = (CSkillType*)x->ptr;
            for (int i = 0; i < x->len; i += 1)
            {
                mapping.Add(cur->id, (
                    Marshal.PtrToStringUTF8((IntPtr)cur->name)!, cur->is_double != 0
                ));
                cur += 1;
            }
            FFI.free_skill_type((void*)x);
        }
        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo info)
        {
            var item = pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            Type pageType = item.PageType;

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType, null, info);
            }
        }

        private void onSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag!, args.RecommendedNavigationTransitionInfo);
            }
        }
    }
}
