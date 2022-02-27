# PetBook

### 编译与运行

- 编译内核

  ```
  cargo build --release
  ```

- 编译UI

  首先安装`ModernWpf`，用NuGet Shell运行

  ```
  Install-Package ModernWpfUI
  ```

  用Visual Studio打开`PetBook.sln`，选择目标为Release，生成=>生成解决方案

- 运行

  将`target\release\core.dll`放入`bin\x64\Release\net6.0-windows10.0.18362.0`文件夹，运行`PetBook.exe`

### 环境

- Visual Studio Community 2022 Preview 17.2.0 Preview 1.0
- rustc 1.59.0 (9d1b2106e 2022-02-23) stable-x86_64-pc-windows-msvc
- Windows >= Windows10 18362
- .NET 6.0

### TODO

早期的数据是硬编码的，不能直接格式化，这部分需要手工解析。

### 许可

MIT

## 贡献者

[![](https://github.com/9646516.png?size=100)](https://github.com/9646516)