# Huawei Technologies Co., Ltd. MU709s


## Интеграционное тестирование модуля "модем"

### Цель

  - произвести тестирование работы модемов MU709s в среде IBox.Shell
  - разработать инструмент быстрого автоматизированного тестирования модуля
  - выполнить рефакторинг и обновление основной кодовой базы IBox.Shell

## Сборка проекта 

```
git clone https://github.com/dgordienko/IBoxUsbModemTest.git
cd \IBoxUsbModemUnitTest\
msbuild IBoxUsbModemTest.sln

```

## Предварительные установки

 - Модем IRZ установлен в терминале
 - Обе карты вставлены в модем и имеют средства на счету

## Конфигурация теста (app.config)

  Конфигурация выполняется путем редактирования секции appSettings в файле app.config согласно следующей схеме

```
  <appSettings>
    <add key="port" value="COM4"/>
    <add key="baudrate" value="9600"/>
  </appSettings>
```


## Запуск в среде Linux

```
# copy \IBoxUsbModemUnitTest\bin\Debug
# copy \IBoxUsbModemTest\packages\xunit.runner.console.2.4.1\tools\net472/

# set alias for command xunit console
alias xunit="mono IBoxUsbModemTest/packages/xunit.runner.console.2.4.1/tools/net472/xunit.console.exe"

# run test
xunit IBoxUsbModemTest/IBoxUsbModemUnitTest/bin/Debug/IBoxUsbModemUnitTest.dll -html report_test.html

```
