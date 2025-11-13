cmake .
cmake --build . --target SerialPortController --config Debug & cmake --build . --target SerialPortController --config Release
sudo cp SerialPortController.so /usr/lib/
sudo ldconfig

