# Release Notes

## [1.1.0] - 2025-05-26
### Changed
- Improved dump output format.
  - Each line now includes:
    - **Offset**: Position in the byte array (hexadecimal)
    - **Hexadecimal values**: Raw byte values grouped by 16 bytes
    - **ASCII representation**: Printable characters; non-printables shown as `.`

#### New format:

```
00000000 | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | 0123456789ABCDEF
-------- | -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ----------------
00000000 | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | ................
00000010 | 10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F | ................
```

#### Old format:

```
00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F
```

## [1.0.0] - 2025-05-17
### Added
- Initial release.
- Added `DumpDebug(this ILogger logger, byte[] bytes)` method to output binary data in hex format.
