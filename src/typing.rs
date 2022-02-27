use std::ffi::{c_void, CString};
use std::mem::ManuallyDrop;
use std::os::raw::c_char;

#[repr(C, align(4))]
pub struct Array {
    pub ptr: *const c_void,
    pub len: u32,
    pub cap: u32,
}

impl Array {
    pub fn new<T>(src: Vec<T>) -> Self {
        let src = ManuallyDrop::new(src);
        Self {
            ptr: src.as_ptr() as *const c_void,
            len: src.len() as u32,
            cap: src.capacity() as u32,
        }
    }
}

#[repr(C)]
pub struct BaseDetail {
    pub id: u32,
    pub name: *const c_char,
}

impl BaseDetail {
    pub fn new(id: u32, name: String) -> Self {
        let str = CString::new(name).unwrap();
        let ptr = str.into_raw();
        Self { id, name: ptr }
    }
}

#[repr(C)]
pub struct PetFullDetail {
    pub base: BaseDetail,
    pub hp: u32,
    pub atk: u32,
    pub def: u32,
    pub spatk: u32,
    pub spdef: u32,
    pub spd: u32,

    pub types: u32,
    pub gender: u32,
    pub evolving_lv: u32,
}

#[repr(C)]
pub struct Effect {
    pub id: i32,
    pub cnt: i32,
    pub args: *const c_char,
    pub description: *const c_char,
}

#[derive(Debug)]
#[repr(C)]
pub struct Skill {
    pub id: i32,
    pub name: *const c_char,
    pub category: i32,
    pub types: i32,
    pub power: i32,
    pub pp: i32,
    pub accuracy: i32,
    pub critical_rate: i32,
    pub effect: *const c_void,
    pub info: *const c_char,
    pub lv: i32,
    pub extra: i32,
}

#[derive(Debug)]
#[repr(C)]
pub struct SkillType {
    pub id: i32,
    pub name: *const c_char,
    pub is_double: u8,
}

#[derive(Debug)]
#[repr(C)]
pub struct MintMark {
    pub id: i32,
    pub name: *const c_char,
    pub atk: i32,
    pub def: i32,
    pub spatk: i32,
    pub spdef: i32,
    pub spd: i32,
    pub hp: i32,
}

pub enum Status {
    UpdateSkillIcon = -3,
    Idle = -2,
    UpdateDatabase = -1,
}

#[derive(Debug)]
#[repr(C)]
pub struct Buff {
    pub id: i32,
    pub effect_id: i32,
    pub tips: *const c_char,
    pub come: *const c_char,
    pub url: *const c_char,
}

pub enum OrderBy {
    Id = 1,
    IdDesc = 2,
    Hp = 4,
    Atk = 8,
    Def = 16,
    Spatk = 32,
    Spdef = 64,
    Spd = 128,
    Sum = 256,
}

impl OrderBy {
    pub fn new(val: i32) -> Self {
        match val {
            1 => OrderBy::Id,
            2 => OrderBy::IdDesc,
            4 => OrderBy::Hp,
            8 => OrderBy::Atk,
            16 => OrderBy::Def,
            32 => OrderBy::Spatk,
            64 => OrderBy::Spdef,
            128 => OrderBy::Spd,
            256 => OrderBy::Sum,

            _ => {
                panic!("bad order")
            }
        }
    }
    pub fn to_str(&self) -> String {
        let o = match *self {
            OrderBy::Hp => "hp desc",
            OrderBy::Atk => "atk desc",
            OrderBy::Def => "def desc",
            OrderBy::Spatk => "spatk desc",
            OrderBy::Spdef => "spdef desc",
            OrderBy::Spd => "spd desc",
            OrderBy::Sum => "hp+atk+def+spatk+spdef+spd desc",
            OrderBy::Id => "id",
            OrderBy::IdDesc => "id desc",
        };
        format!("order by {}", o)
    }
}
