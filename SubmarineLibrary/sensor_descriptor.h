#pragma once
#include "stdafx.h"
#include "isensor.h"

namespace zubmarine
{
	namespace event_layer
	{
		struct SensorDescriptor
		{
			std::shared_ptr<input::sensor::ISensor> sensor;
			i32 slot = 0;
			i32 queueSize = 0;
		};
	}
}