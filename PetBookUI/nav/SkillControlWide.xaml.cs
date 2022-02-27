using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using PetBook.typing;
namespace PetBook.nav
{
    public partial class SkillControlWide : UserControl
    {
        public SkillControlWide(Skill skill)
        {
            InitializeComponent();
            string icon;
            if (skill.category != 4) icon = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, Path.Combine(MainWindow.blob_base, String.Format("type_{0}.png", skill.types)));
            else icon = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, Path.Combine(MainWindow.blob_base, String.Format("prop.png", skill.types)));
            this.head.Source = new BitmapImage(new Uri(icon, UriKind.Absolute));
            this.name.Text = skill.name;
            this.id.Text = skill.id.ToString();
            this.category.Text = skill.category switch
            {
                1 => "物理攻击",
                2 => "特殊攻击",
                _ => "属性攻击",
            };
            this.types_string.Text = skill.types_string;
            this.power.Text = String.Format("威力:{0}", skill.power);
            this.pp.Text = String.Format("PP:{0}", skill.pp);
            this.accuracy.Text = String.Format("命中:{0}%", skill.accuracy);
            this.critical_rate.Text = skill.critical_rate switch
            {
                0 => "暴击:-",
                _ => String.Format("暴击:{0}%", skill.critical_rate),
            };
            this.lv.Text = String.Format("学习:{0}", skill.lv != 0 ? skill.lv.ToString() : "-");
            if (skill.extra != 0)
            {
                this.extra.Text = "第五技能";
            }
            this.tips.Text = "文本:" + ((skill.info == null || skill.info.Length == 0 || skill.info.Equals("null")) ? "-" : skill.info);
            foreach (Effect effect in skill.effects)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("id ");
                sb.AppendLine(effect.id.ToString());
                sb.Append("参数量 ");
                sb.AppendLine(effect.cnt.ToString());
                sb.Append(PetBook.core.utils.parse_effect(effect.id, effect.cnt, effect.desc, effect.args));

                this.layout.Children.Add(new Border
                {
                    Width = 450,
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    Margin = new Thickness(3, 3, 3, 3),
                    Child = new TextBlock
                    {
                        Text = sb.ToString(),
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 15,
                        Width = 440,
                        Margin = new Thickness(3, 3, 3, 3)
                    }
                });
            }
        }
    }
}
