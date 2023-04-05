# ClientOnamaeDdns

![aaa](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)

お名前ドットコム DDNS クライアント

## 使用方法

```
> ClientOnamaeDdns.exe --user_id="12345678" --password="abcdefghijklmn" --hostname="www" --domainname="example.com"
```

|引数名|値|
|-|-|
|user_id|お名前ドットコムの会員ID|
|password|お名前ドットコムのパスワード|
|hostname|DDNS に通知したいサブドメイン名|
|domainname|DDNS に通知したいドメイン名|

## 動作詳細

1. api.ipify.org を使用して自分のグローバル IP アドレスを取得します。
2. (2回目以降の実行の場合) 取得した IP と `%APPDATA%\ClientDdns\ip.txt` に保存されている IP が一致した場合は処理を終了します。
3. グローバル IP をお名前ドットコムの DDNS サーバーに通知します。
4. 通知したグローバル IP を `%APPDATA%\ClientDdns\ip.txt` に保存します。
5. 現在時刻と通知したグローバル IP を `%APPDATA%\ClientDdns\log.txt` に追記します。
