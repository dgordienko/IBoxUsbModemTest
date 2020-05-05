# Huawei Technologies Co., Ltd. MU709s

## Enviroment

```
# set alias for command xunit console
alias xunit="mono IBoxUsbModemTest/packages/xunit.runner.console.2.4.1/tools/net472/xunit.console.exe"

# run test
xunit IBoxUsbModemTest/IBoxUsbModemUnitTest/bin/Debug/IBoxUsbModemUnitTest.dll -html report_test.html

```

### Configuration (app.config)

```
  <appSettings>
    <add key="port" value="COM4"/>
    <add key="baudrate" value="9600"/>
  </appSettings>
```
