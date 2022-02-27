use crate::{Array, Effect, Skill};
use std::ffi::CString;
use std::os::raw::c_char;

pub unsafe fn drop_skill(i: &Skill) {
    std::mem::drop(CString::from_raw(i.info as *mut c_char));
    std::mem::drop(CString::from_raw(i.name as *mut c_char));
    let a: Box<Array> = std::mem::transmute(i.effect);
    let b = Vec::from_raw_parts(a.ptr as *mut Effect, a.len as usize, a.cap as usize);
    for j in b {
        std::mem::drop(CString::from_raw(j.description as *mut c_char));
        std::mem::drop(CString::from_raw(j.args as *mut c_char));
    }
}
