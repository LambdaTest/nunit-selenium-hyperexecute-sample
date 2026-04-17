Get-ChildItem -Path NUnitHyperTestDemo -Recurse -Filter *.cs |
    Where-Object { $_.Name -notmatch 'DriverFactory|AssemblyInfo' } |
    Select-String -Pattern 'Category\((.+)\)' |
    ForEach-Object { $_.Matches[0].Groups[1].Value }
