using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PetBook.core;
namespace PetBook.typing
{
    public class IntStringPair
    {
        public IntStringPair(int id, string name)
        {
            this.i = id;
            this.s = name;
        }

        public int i { get; set; }
        public string s { get; set; }
    }

    unsafe public class Effect
    {
        public int id;
        public int cnt;
        public string desc;
        public string args;
        public Effect(CEffect* o)
        {
            this.id = o->id;
            this.cnt = o->cnt;
            this.desc = Marshal.PtrToStringUTF8((IntPtr)o->description)!;
            this.args = Marshal.PtrToStringUTF8((IntPtr)o->args)!;
        }
    }

    unsafe public class Skill
    {
        public int id { get; set; }
        public string name { get; set; }

        public int category { get; set; }
        public int types { get; set; }
        public String types_string
        {
            get { return MainWindow.mapping[types].Item1; }
        }
        public int power { get; set; }
        public int pp { get; set; }
        public int accuracy { get; set; }
        public int critical_rate { get; set; }

        public List<Effect> effects { get; set; }

        public string info { get; set; }
        public int lv { get; set; }
        public int extra { get; set; }
        public Skill(CSkill* cur)
        {
            Int32 id = cur->id;
            string name = Marshal.PtrToStringUTF8((IntPtr)cur->name)!;

            Int32 category = cur->category;
            Int32 types = cur->types;
            Int32 power = cur->power;
            Int32 pp = cur->pp;
            Int32 accuracy = cur->accuracy;
            Int32 critical_rate = cur->critical_rate;

            List<Effect> effects = new List<Effect>();
            CArray* ptr = (CArray*)cur->effect;
            CEffect* o = (CEffect*)ptr->ptr;
            for (int j = 0; j < ptr->len; j += 1)
            {
                effects.Add(new Effect(o));
                o += 1;
            }

            string info = Marshal.PtrToStringUTF8((IntPtr)cur->info)!;
            Int32 lv = cur->lv;
            Int32 extra = cur->extra;
            this.id = id;
            this.name = name;
            this.category = category;
            this.types = types;
            this.power = power;
            this.accuracy = accuracy;
            this.pp = pp;
            this.critical_rate = critical_rate;
            this.effects = effects;
            this.info = info;
            this.lv = lv;
            this.extra = extra;
        }
    }

    public class Stringx3
    {
        public string s1 { get; set; }
        public string s2 { get; set; }
        public string s3 { get; set; }

        public Stringx3(string s1, string s2, string s3)
        {
            this.s1 = s1;
            this.s2 = s2;
            this.s3 = s3;
        }

    }

    public struct MintMark
    {
        public int id { get; set; }
        public string name { get; set; }

        public int hp { get; set; }
        public int atk { get; set; }
        public int def { get; set; }
        public int spatk { get; set; }
        public int spdef { get; set; }
        public int spd { get; set; }
        public int sum { get; set; }
        public unsafe MintMark(CMintMark* x)
        {
            this.id = x->id;
            this.name = Marshal.PtrToStringUTF8((IntPtr)x->name)!;
            this.hp = x->hp;
            this.spd = x->spd;
            this.atk = x->atk;
            this.def = x->def;
            this.spatk = x->spatk;
            this.spdef = x->spdef;
            this.sum = x->hp + x->spd + x->atk + x->def + x->spatk + x->spdef;
        }
    }

}
