fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

## iOS

### ios sync_dev_certs

```sh
[bundle exec] fastlane ios sync_dev_certs
```

同步开发证书 (Development)

### ios sync_appstore_certs

```sh
[bundle exec] fastlane ios sync_appstore_certs
```

同步 App Store 证书 (Distribution)

### ios sync_adhoc_certs

```sh
[bundle exec] fastlane ios sync_adhoc_certs
```

同步 Ad Hoc 证书 (内部测试)

### ios sync_all_certs

```sh
[bundle exec] fastlane ios sync_all_certs
```

同步所有证书

### ios create_dev_cert

```sh
[bundle exec] fastlane ios create_dev_cert
```

创建新的开发证书

### ios create_appstore_cert

```sh
[bundle exec] fastlane ios create_appstore_cert
```

创建新的 App Store 证书

### ios build_release

```sh
[bundle exec] fastlane ios build_release
```

构建 Release 版本 (App Store)

### ios build_adhoc

```sh
[bundle exec] fastlane ios build_adhoc
```

构建 Ad Hoc 版本 (内部测试)

### ios upload_testflight

```sh
[bundle exec] fastlane ios upload_testflight
```

上传到 TestFlight

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
