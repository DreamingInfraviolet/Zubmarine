#pragma once
#include "isensor.h"
#include "export.h"

namespace zubmarine
{
	namespace sensor
	{
		class ZUBMARINE_EXPORTS_API CompassSensor : public ISensor
		{
		public:

			int x();
		};
	}
}