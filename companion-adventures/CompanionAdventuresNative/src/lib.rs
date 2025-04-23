use std::ffi::{c_char, CStr, CString};

#[unsafe(no_mangle)]
pub fn say_hello(name: *const c_char) -> *const c_char {
    let text;
    unsafe {
        text = CStr::from_ptr(name).to_str().unwrap();
    }
    CString::new(format!("Hello {text}!")).unwrap().into_raw()
}

#[unsafe(no_mangle)]
pub fn version() -> *const c_char {
    CString::new("CompanionAdventures 1.0.0").unwrap().into_raw()
}

#[unsafe(no_mangle)]
pub fn free_string(ptr: *mut c_char) {
    if ptr.is_null() {
        return;
    }
    unsafe {
        let _ = CString::from_raw(ptr);
    }
}

#[unsafe(no_mangle)]
pub fn add(left: i32, right: i32) -> i32 {
    left + right
}

#[unsafe(no_mangle)]
pub fn subtract(left: i32, right: i32) -> i32 {
    left - right
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn it_works() {
        let result = add(2, 2);
        assert_eq!(result, 4);
    }
}
