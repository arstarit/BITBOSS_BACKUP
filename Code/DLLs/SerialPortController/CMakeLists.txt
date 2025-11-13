# Version mínima de CMake
cmake_minimum_required(VERSION 3.0)
#Nombre del proyecto
project(SerialPortController)
#Añadimos los ficheros y le decimos que sera una librería compartida
add_library(SerialPortController SHARED spcontroller.cpp spcontroller.h)
#Quitamos los prefijos (esto quita el "lib" que añade)
set_target_properties(SerialPortController PROPERTIES PREFIX "")
#Indicamos el nombre de la salida
set_target_properties(SerialPortController PROPERTIES OUTPUT_NAME SerialPortController)
