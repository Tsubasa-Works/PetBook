#![allow(clippy::missing_safety_doc)]

extern crate core;

mod manager;
mod typing;
mod utils;

use manager::*;
use std::borrow::Borrow;
use std::ffi::{c_void, CStr, CString};
use std::os::raw::c_char;
use std::sync::Once;
use typing::*;
use utils::drop_skill;

static INIT: Once = Once::new();
static mut SINGLETON: *mut PetBookManager = std::ptr::null_mut();

#[no_mangle]
pub unsafe extern "cdecl" fn create_model() {
    INIT.call_once(|| unsafe {
        let ret = Box::new(PetBookManager::new());
        SINGLETON = std::mem::transmute(Box::leak(ret));
    });
}

#[no_mangle]
pub unsafe extern "cdecl" fn update() {
    SINGLETON.as_mut().unwrap().update();
}

#[no_mangle]
pub unsafe extern "cdecl" fn status() -> i32 {
    SINGLETON.as_ref().unwrap().status
}

/// free method
#[no_mangle]
pub unsafe extern "cdecl" fn free_pet_detail(x: *mut c_void) {
    let arr: Box<PetFullDetail> = std::mem::transmute(x);
    let ptr = arr.base.name as *mut c_char;
    let s = CString::from_raw(ptr);
    std::mem::drop(s);
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_int_string_pair_list(x: *mut c_void) {
    let arr: Box<Array> = std::mem::transmute(x);
    let x = Vec::from_raw_parts(
        arr.ptr as *mut BaseDetail,
        arr.len as usize,
        arr.cap as usize,
    );
    for i in x {
        let ptr = i.name as *mut c_char;
        let s = CString::from_raw(ptr);
        std::mem::drop(s);
        std::mem::drop(i);
    }
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_pet_skill(x: *const c_void) {
    let arr: Box<Array> = std::mem::transmute(x);
    let x = Vec::from_raw_parts(arr.ptr as *mut Skill, arr.len as usize, arr.cap as usize);
    for i in x.iter() {
        drop_skill(i);
    }
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_single_skill(x: *const c_void) {
    let i: Box<Skill> = std::mem::transmute(x);
    drop_skill(i.borrow());
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_pet_buff(x: *const c_void) {
    let arr: Box<Array> = std::mem::transmute(x);
    let x = Vec::from_raw_parts(arr.ptr as *mut Buff, arr.len as usize, arr.cap as usize);
    for i in x {
        std::mem::drop(CString::from_raw(i.tips as *mut c_char));
        std::mem::drop(CString::from_raw(i.come as *mut c_char));
        std::mem::drop(CString::from_raw(i.url as *mut c_char));
    }
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_skill_type(x: *const c_void) {
    let arr: Box<Array> = std::mem::transmute(x);
    let x = Vec::from_raw_parts(
        arr.ptr as *mut SkillType,
        arr.len as usize,
        arr.cap as usize,
    );
    for i in x {
        std::mem::drop(CString::from_raw(i.name as *mut c_char));
    }
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_buff(x: *const c_void) {
    let arr: Box<Buff> = std::mem::transmute(x);
    std::mem::drop(CString::from_raw(arr.url as *mut c_char));
    std::mem::drop(CString::from_raw(arr.tips as *mut c_char));
    std::mem::drop(CString::from_raw(arr.come as *mut c_char));
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_mintmark_list(x: *const c_void) {
    let arr: Box<Array> = std::mem::transmute(x);
    let x = Vec::from_raw_parts(arr.ptr as *mut MintMark, arr.len as usize, arr.cap as usize);
    for i in x {
        let ptr = i.name as *mut c_char;
        let s = CString::from_raw(ptr);
        std::mem::drop(s);
        std::mem::drop(i);
    }
}

/// query method
#[no_mangle]
pub unsafe extern "cdecl" fn query_skill_detail(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_skill_detail(idx);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_pet_detail(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_pet_detail(idx);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_skill_list() -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_skill_list();
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_skill_effect_related(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_skill_effect_related(idx);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn free_skill_effect(x: *const c_void) {
    let arr: Box<BaseDetail> = std::mem::transmute(x);
    std::mem::drop(CString::from_raw(arr.name as *mut c_char));
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_buff_related(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_buff_related(idx);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_skill_type() -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_skill_type();
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_skill_related(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_skill_related(idx);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_pet_buff(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_pet_buff(idx);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_pet_skill(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_pet_skill(idx);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_skill_list_with_condition(str: *const c_void) -> *const c_void {
    let s = CStr::from_ptr(str as *mut c_char);
    let sb = s.to_str().unwrap();
    let sb = String::from_utf8(Vec::from(sb)).unwrap();
    let ret = SINGLETON
        .as_ref()
        .unwrap()
        .query_skill_list_with_condition(&sb);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_skill_effect(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_skill_effect(idx);
    if let Some(x) = ret {
        std::mem::transmute(Box::leak(Box::new(x)))
    } else {
        std::ptr::null()
    }
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_buff(idx: i32) -> *const c_void {
    let ret = SINGLETON.as_ref().unwrap().query_buff(idx);
    if let Some(x) = ret {
        std::mem::transmute(Box::leak(Box::new(x)))
    } else {
        std::ptr::null()
    }
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_pet_list_with_condition(
    str: *const c_void,
    order: i32,
) -> *const c_void {
    let s = CStr::from_ptr(str as *mut c_char);
    let sb = s.to_str().unwrap();
    let sb = String::from_utf8(Vec::from(sb)).unwrap();

    let order = OrderBy::new(order);
    let ret = SINGLETON
        .as_ref()
        .unwrap()
        .query_pet_list_with_condition(&sb, order);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_pet_list(order: i32) -> *const c_void {
    let order = OrderBy::new(order);
    let ret = SINGLETON.as_ref().unwrap().query_pet_list(order);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_mintmark_list(order: i32) -> *const c_void {
    let order = OrderBy::new(order);
    let ret = SINGLETON.as_ref().unwrap().query_mintmark_list(order);
    std::mem::transmute(Box::leak(Box::new(ret)))
}

#[no_mangle]
pub unsafe extern "cdecl" fn query_mintmark_list_with_condition(
    str: *const c_void,
    order: i32,
) -> *const c_void {
    let s = CStr::from_ptr(str as *mut c_char);
    let sb = s.to_str().unwrap();
    let sb = String::from_utf8(Vec::from(sb)).unwrap();

    let order = OrderBy::new(order);
    let ret = SINGLETON
        .as_ref()
        .unwrap()
        .query_mintmark_list_with_condition(&sb, order);
    std::mem::transmute(Box::leak(Box::new(ret)))
}
