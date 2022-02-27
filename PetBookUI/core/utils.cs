using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace PetBook.core
{
    public class utils
    {
        private static readonly string[] vp = new string[] { "攻击", "防御", "特攻", "特防", "速度", "命中" };
        private static readonly string[] vp2 = new string[] { "麻痹", "中毒", "烧伤", "吸取对方的体力", "被对方吸取体力", "冻伤", "害怕", "疲惫", "睡眠", "石化", "混乱", "衰弱", "山神守护", "易燃", "狂暴", "冰封", "流血", "XX", "XX", "瘫痪", "失明", "XX", "焚烬", "诅咒", "XX", "XX", "XX", "感染", "束缚", "失神", "沉默" };
        static unsafe public string parse_effect(int id, int cnt, string desc, string args)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string p3;
            List<int> v = new List<int>();
            if (cnt > 0)
            {
                foreach (string i in args.Split(' '))
                {
                    v.Add(int.Parse(i));
                }
            }
            switch (id)
            {
                case 4:
                    p3 = (v[2] > 0) ? String.Format("+{0}", v[2]) : v[2].ToString();
                    sb.Append(String.Format("技能使用成功时，{0}%自身{1}等级{2}", v[1], vp[v[0]], p3));
                    break;
                case 5:
                    p3 = (v[2] > 0) ? String.Format("+{0}", v[2]) : v[2].ToString();
                    sb.Append(String.Format("技能使用成功时，{0}%对方{1}等级{2}", v[1], vp[v[0]], p3));
                    break;
                case 59:
                    sb.Append(String.Format("消耗自身全部体力(体力降到0), 使下一只出战精灵的 {0} 和 {1} 能力提升1个等级", vp[v[0]], vp[v[1]]));
                    break;
                case 65:
                    sb.Append(String.Format("{0}回合内，{1}系技能威力为{2}倍", v[0], MainWindow.mapping[v[1]].Item1, v[2]));
                    break;
                case 107:
                    sb.Append(String.Format("若本次攻击造成的伤害小于{0} 则自身{1}等级提升1个等级", v[0], vp[v[1]]));
                    break;
                case 110:
                    sb.Append(String.Format("{0}回合内，每次躲避攻击都有{1}%几率使自身{2}等级提升1个等级", v[0], v[1], vp[v[2]]));
                    break;
                case 122:
                    sb.Append(String.Format("先出手时，{0}%对方{1}等级降低{2}", v[1], vp[v[0]], v[2]));
                    break;
                case 123:
                    sb.Append(String.Format("{0}回合内，受到任何伤害，自身{1}提高{2}个等级", v[0], vp[v[1]], v[2]));
                    break;
                case 147:
                    sb.Append(String.Format("后出手时，{0}%概率使对方{1}", v[0], vp2[v[1]]));
                    break;
                case 148:
                    sb.Append(String.Format("后出手时，{0}%使对方{1}降低{2}个等级", v[1], vp[v[0]], v[2]));
                    break;
                case 149:
                    sb.Append(String.Format("命中后，{0}%令对方{1}，{2}%令对方{3}", v[0], vp2[v[1]], v[2], vp2[v[3]]));
                    break;
                case 152:
                    sb.Append(String.Format("{0}回合内，若对方使用属性技能，则{1}%使对方{2}", v[0], v[1], vp2[v[2]]));
                    break;
                case 154:
                    sb.Append(String.Format("若对手{0}，则对对方造成伤害的1/{1}恢复自身体力", vp2[v[0]], v[1]));
                    break;
                case 158:
                    sb.Append(String.Format("当次攻击击败对手，则{0}%自身{1}等级+{2}", v[1], vp[v[0]], v[2]));
                    break;
                case 159:
                    sb.Append(String.Format("自身体力小于最大值的1/{0}时，{1}%几率令对方{2}", v[0], v[1], vp2[v[2]]));
                    break;
                case 166:
                    sb.Append(String.Format("{0}回合内，若对手使用属性攻击则{1}%对手{2}等级{3}", v[0], v[2], vp[v[1]], v[3]));
                    break;
                case 169:
                    sb.Append(String.Format("{0}回合内，每回合额外附加{1}% 几率令对手{2}", v[0], v[1], vp2[v[2]]));
                    break;
                case 173:
                    sb.Append(String.Format("先出手时，{0}%概率令对方{1}", v[0], vp2[v[1]]));
                    break;
                case 174:
                    sb.Append(String.Format("{0}回合内，若对手使用属性攻击则{1}%自身{2}等级+{3}", v[0], v[3], vp[v[1]], v[4]));
                    break;
                case 175:
                    sb.Append(String.Format("若对手处于异常状态,则{0}%自身{1}等级+{2}", v[1], vp[v[0]], v[2]));
                    break;
                case 181:
                    sb.Append(String.Format("{0}%几率令对手{1}，连续攻击每次提高{2}%几率最高提高{3}%", v[0], vp2[v[1]], v[2], v[3]));
                    break;
                case 182:
                    sb.Append(String.Format("若对手处于{0}状态，{1}%自身{2}等级+{3}", vp2[v[0]], v[2], vp[v[1]], v[3]));
                    break;
                case 183:
                    sb.Append(String.Format("{0}回合内免疫并反弹{1}伤害", v[0], v[1] == 1 ? "物理" : "特殊"));
                    break;
                case 184:
                    sb.Append(String.Format("若对手处于能力提升状态,则{0}%自身{1}等级+{2}", v[1], vp[0], v[2]));
                    break;
                case 185:
                    sb.Append(String.Format("若击败{0}的对手，则下一个出场的对手也进入{1}状态", vp2[v[0]], vp2[v[0]]));
                    break;
                case 186:
                    sb.Append(String.Format("后出手时，{0}%使自身{1}提升{2}个等级", v[1], vp[v[0]], v[2]));
                    break;
                case 193:
                    sb.Append(String.Format("若对手{0}，则必定致命一击", vp2[v[0]]));
                    break;
                case 194:
                    sb.Append(String.Format("造成伤害的1/{0}回复自身体力，若对手{1}，则造成伤害的1/{2}回复自身体力", v[0], vp2[v[1]], v[2]));
                    break;
                case 196:
                    sb.Append(String.Format("{0}%令对方{1}等级{2}；若先出手，则{3}%使对方{4}等级{5}", v[1], vp[v[0]], v[2], v[4], vp[v[3]], v[5]));
                    break;
                case 199:
                    sb.Append(String.Format("被击败后，下一个出场的精灵{0}等级+{1}", vp[v[0]], v[1]));
                    break;
                case 523:
                    sb.Append("若当回合未击败对手，则自身");
                    for (int i = 0; i < 6; i += 1)
                    {
                        if (v[i] > 0)
                        {
                            sb.Append(vp[i]);
                        }
                    }
                    sb.Append("能力+1");
                    break;
                case 585:
                    sb.Append("技能使用成功时,");
                    for (int i = 0; i < 6; i += 1)
                    {
                        if (v[i] > 0)
                        {
                            sb.Append(vp[i]);
                            sb.Append("等级上升");
                            sb.Append(v[i]);
                            if (i != 5)
                            {
                                sb.Append(',');
                            }
                        }
                    }
                    sb.Append("能力+1");
                    break;
                case 737:
                    sb.Append(String.Format("{0}% 使对手{1}，若没有触发，则回合结束时减少对手1 /{2}最大体力", v[0], vp2[v[1]], v[2]));
                    break;
                case 1181:
                    sb.Append(String.Format("{0}回合内每回合{1}%令对手{2}，未触发则对手全属性-{3}", v[0], v[1], vp2[v[2]], v[3]));
                    break;
                case 1187:
                    sb.Append(String.Format("命中则有{0}%几率使对手{1}，未触发则消除对手回合类效果", v[0], vp2[v[1]]));
                    break;
                case 1188:
                    sb.Append(String.Format("{0}回合内自身受到攻击则使对手{1}，未触发则吸取对手最大体力的1/{2}", v[0], vp2[v[1]], v[2]));
                    break;
                case 1195:
                    sb.Append(String.Format("{0}回合内自身受到攻击{1}%使对手{2}，若未触发则对手受到{3}点固定伤害", v[0], v[1], vp2[v[2]], v[3]));
                    break;

                default:
                    if (cnt == 0)
                    {
                        sb.Append(desc);
                    }
                    else if (cnt <= 2)
                    {
                        bool fin = false;
                        if (desc.Contains("{0}"))
                        {
                            try
                            {

                                string s = String.Format(desc, v.Select(x => x.ToString()).ToArray());
                                sb.Append(s);
                                fin = true;
                            }
                            catch (Exception)
                            {

                            }
                        }

                        if (!fin)
                        {
                            int done = 0;
                            foreach (char i in desc)
                            {

                                if (i == 'n' || i == 'm' || i == 'k')
                                {
                                    if (done >= cnt)
                                    {
                                        sb.Append(i);
                                    }
                                    else
                                    {
                                        sb.Append(v[done].ToString());
                                        done += 1;
                                    }
                                }
                                else
                                {
                                    sb.Append(i);
                                }
                            }
                        }

                    }
                    else
                    {
                        sb.Append("模板 ");
                        sb.Append(desc);
                        if (cnt > 0)
                        {
                            sb.AppendLine();
                            sb.Append("参数 ");
                            sb.Append(args);
                        }
                    }
                    break;
            }
            return sb.ToString();
        }
        static unsafe public List<string> get_buff(int idx)
        {
            CArray* x = (CArray*)FFI.query_pet_buff(idx);
            CBuff* cur = (CBuff*)x->ptr;

            List<string> vcc = new List<string>();
            for (int i = 0; i < x->len; i += 1)
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
                sb.Append(s3);
                if (url != "null")
                {
                    sb.AppendLine(url);
                }
                if (come != "null")
                {
                    sb.AppendLine(come);
                }

                vcc.Add(sb.ToString());
                cur += 1;
            }
            FFI.free_pet_buff((void*)x);
            return vcc;
        }
    }
}
