#pragma once
#include "export.h"

namespace zubmarine
{
	namespace sensor
	{
		class ZUBMARINE_EXPORTS_API ISensor
		{
		protected:
			virtual ~ISensor() {}
		};
	}
}