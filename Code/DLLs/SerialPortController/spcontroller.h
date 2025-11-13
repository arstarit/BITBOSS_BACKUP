#pragma once

//Utilizamos directivas de preprocesado para definir la macro de la API
//Esto hay que hacerlo porque en Windows y en NoWindows se declaran diferente
#ifdef _WIN32
#  ifdef MODULE_API_EXPORTS
#    define MODULE_API extern "C" __declspec(dllexport) 
#  else
#    define MODULE_API extern "C" __declspec(dllimport)
#  endif
#else
#  define MODULE_API extern "C"
#endif
//Declaracion de los métodos nativos

//         [DllImport("SerialPortController", EntryPoint = "OpenPort")]
MODULE_API int OpenPort(char const *portname);

//         [DllImport("SerialPortController", EntryPoint = "ClosePort")]
MODULE_API void ClosePort(int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "readBytes")]
MODULE_API int readBytes(unsigned char message[], int maxsize, int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "writeBytes")]
MODULE_API void writeBytes(unsigned char message[],  int maxsize, int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "set_wakeup")]
MODULE_API int set_wakeup(int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "set_space")]
MODULE_API int set_space(int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "readSpaceBytes")]
MODULE_API int readSpaceBytes(unsigned char message[], int maxsize, int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "readSpaceBytes2")]
MODULE_API int readSpaceBytes2(unsigned char message[], int maxsize, int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "writeBytesWithWakeup")]
MODULE_API void writeBytesWithWakeup(unsigned char byteWithWakeup[], unsigned char bytesWithSpace[], int maxsize, int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "flushPort")]
MODULE_API void flushPort(int PortFileDescriptor);

//        [DllImport("SerialPortController", EntryPoint = "flushOutputPort")]
MODULE_API void flushOutputPort(int PortFileDescriptor);

