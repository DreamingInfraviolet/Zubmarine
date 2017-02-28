#include "stdafx.h"
#include "dummy_hardware_layer.h"

namespace zubmarine
{
	namespace hardware_layer
	{
		void DummyHardwareLayer::threadFunction(DummyHardwareLayer& self)
		{
			while (!self.halt)
			{
				// Execute callback 
				for (int i = 0; i < self.numberOfSlots(); ++i)
				{
					i32 value = 0;
					self.updateCallback(i, reinterpret_cast<byte*>(&value), sizeof(value));
				}

				// Sleep to reach desired rate
				self.rate.sleep();
			}
		}

		DummyHardwareLayer::DummyHardwareLayer(UpdateCallbackType callback, const util::Rate& rate)
			: rate(rate)
		{
			this->setUpdateCallback(callback);

			// Start thread
			this->thread = std::thread([this]() { DummyHardwareLayer::threadFunction(*this); });
		}

		i32 DummyHardwareLayer::numberOfSlots()
		{
			return 2;
		}

		DummyHardwareLayer::~DummyHardwareLayer()
		{
			this->halt = true;
			if (this->thread.joinable())
			{
				this->thread.join();
			}
		}
	}
}