#pragma once

#ifdef ZUBMARINE_EXPORTS  
#define ZUBMARINE_EXPORTS_API __declspec(dllexport)   
#else
#define ZUBMARINE_EXPORTS_API __declspec(dllimport)   
#endif