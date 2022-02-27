using System;
using System.Runtime.InteropServices;

namespace PetBook.core
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CArray
    {
        public void* ptr;
        public UInt32 len;
        public UInt32 cap;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CBaseDetail
    {
        public UInt32 id;
        public void* name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CPetFullDetail
    {
        public CBaseDetail bases;
        public UInt32 hp;
        public UInt32 atk;
        public UInt32 def;
        public UInt32 spatk;
        public UInt32 spdef;
        public UInt32 spd;

        public UInt32 types;
        public UInt32 gender;
        public UInt32 evolving_lv;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CEffect
    {
        public Int32 id;
        public Int32 cnt;
        public void* args;
        public void* description;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CSkill
    {
        public Int32 id;
        public void* name;
        public Int32 category;
        public Int32 types;
        public Int32 power;
        public Int32 pp;
        public Int32 accuracy;
        public Int32 critical_rate;
        public void* effect;
        public void* info;
        public Int32 lv;
        public Int32 extra;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CSkillType
    {
        public Int32 id;
        public void* name;
        public Byte is_double;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CBuff
    {
        public Int32 id;
        public Int32 effect_id;
        public void* tips;
        public void* come;
        public void* url;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CMintMark
    {
        public Int32 id;
        public void* name;

        public Int32 atk;
        public Int32 def;
        public Int32 spatk;
        public Int32 spdef;
        public Int32 spd;
        public Int32 hp;
    }


    unsafe public class FFI
    {
        const string dll = "core.dll";

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void create_model();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void update();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_pet_list(int order);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_mintmark_list(int order);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_skill_type();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_pet_list_with_condition(void* str, int order);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_mintmark_list_with_condition(void* str, int order);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_pet_detail(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 status();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_pet_skill(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_pet_buff(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_skill_list_with_condition(void* str);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_skill_list();

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_skill_detail(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_skill_related(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_skill_effect(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_buff(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_skill_effect_related(Int32 idx);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* query_buff_related(Int32 idx);



        //query_buff
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_buff(void* x);

        //query_pet_buff
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_pet_buff(void* x);

        //query_pet_list，query_pet_list_with_condition，query_skill_list,
        //query_skill_list_with_condition，query_skill_effect_related,query_buff_related
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_int_string_pair_list(void* x);

        //query_pet_detail
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_pet_detail(void* x);

        //query_pet_skill
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_pet_skill(void* x);

        //query_skill_detail
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_single_skill(void* x);

        //query_skill_type
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_skill_type(void* x);

        //query_skill_effect
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_skill_effect(void* x);

        //query_mintmark_list,query_mintmark_list_with_condition
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_mintmark_list(void* x);
    }
}
