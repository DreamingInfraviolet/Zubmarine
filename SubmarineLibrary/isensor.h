#pragma once
#include <vector>
#include <memory>

#include "export.h"
#include "iinput.h"
#include "isensor_reading.h"
#include "hardware_event_queue.h"

namespace zubmarine
{
	namespace input
	{
		namespace sensor
		{
			class ZUBMARINE_EXPORTS_API ISensor : public IInput
			{
			protected:
				virtual ~ISensor() {}

			public:

				using SensorReturnType = std::vector<std::shared_ptr<ISensorReading>>;

				virtual SensorReturnType sense() = 0;
				virtual void update(const event_layer::HardwareEventQueue::EventType&) = 0;
			};
		}
	}
}