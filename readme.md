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

Установка модема в OC ArchLinux
~~~
 # проверка подключенного порта 
 ls -l /dev/ | grep USB
~~~

В случае, если не обнаружены подключенные устройства выполнить команду

~~~
lsusb | grep MU709
~~~

Результатом выполнения команды будет вывод следющего вида
~~~
Bus 003 Device 004: ID 12d1:1c25 Huawei Technoligies Co., Ltd. MU709
~~~
Создать, или скопировать файл с правилами

~~~
vim /etc/udev/rules.d/10-mu709.rules
~~~
со следующим содержимым
~~~
ACTION=="add", SUBSYSTEM=="usb", ATTRS{idVendor}=="12d1", ATTRS{idProduct}=="1c25", MODE="0666", SYMLINK="ttyUSB0"
~~~

Перезагрузить udev и выполнить проверку порта.
~~~
udevadm control --reload-rules

ls -l /dev/ | grep USB
~~~